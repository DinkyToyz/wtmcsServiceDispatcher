#!perl

use strict;
use Text::CSV;
use YAML;
use DBI;

my $db = DBI->connect('dbi:SQLite:dbname=wtmcsServiceDispatcherDebugLog.db',"","");
$db->do('PRAGMA journal_mode = MEMORY');

my $records = 0;
my %data = ();
my $scsv = Text::CSV->new({sep_char=>';', allow_whitespace=>1, blank_is_undef=>1});
die unless (open(F,'<:encoding(UTF-8)', 'wtmcsServiceDispatcher.log'));

print "< wtmcsServiceDispatcher.log\n";
while (my $l = <F>)
{
    next unless ($l =~ /^.*<(Vehicles|Buildings)\.DebugListLog>(.*?)[\r\n]*$/);
    my $type = $1;
    next unless ($scsv->parse($2));

    $data{$type} = {c=>{}, r=>[]} unless ($data{$type});
    my %record = ();
    my $p = 0;
    foreach my $fld ($scsv->fields)
    {
        next unless ($fld =~ /^([a-zA-Z]+)=(.*)$/);

        my $col = $1;
        my $val = $2;
        $val =~ s/^'(.*?)'$/$1/;

        if ($data{$type}->{c}->{$col})
        {
            $data{$type}->{c}->{$col}->{p} = $p if ($p > $data{$type}->{c}->{$col}->{p});
        }
        else
        {
            $data{$type}->{c}->{$col} = {p=>$p, t=>'NULL'};
        }

        unless ($data{$type}->{c}->{$col}->{t} eq 'TEXT')
        {
            if ($val =~ /^(?:false|true|\d+)$/i)
            {
                if ($data{$type}->{c}->{$col}->{t} eq 'NULL')
                {
                    $data{$type}->{c}->{$col}->{t} = 'INTEGER';
                }
                elsif ($data{$type}->{c}->{$col}->{t} ne 'INTEGER' && $data{$type}->{c}->{$col}->{t} ne 'REAL')
                {
                    $data{$type}->{c}->{$col}->{t} = 'TEXT';
                }
            }
            elsif ($val =~ /^(?:\d+\.\d*|\.\d+)/)
            {
                if ($data{$type}->{c}->{$col}->{t} eq 'NULL' || $data{$type}->{c}->{$col}->{t} eq 'INTEGER')
                {
                    $data{$type}->{c}->{$col}->{t} = 'REAL';
                }
                elsif ($data{$type}->{c}->{$col}->{t} ne 'REAL')
                {
                    $data{$type}->{c}->{$col}->{t} = 'TEXT';
                }
            }
            elsif ($val !~ /^\s*$/)
            {
                $data{$type}->{c}->{$col}->{t} = 'TEXT';
            }
        }

        $record{$col} = $val;

        $p ++;
    }
    push @{$data{$type}->{r}}, \%record;
    $records ++;
}
close(F);
print "# $records\n";

if (open(F, '>:encoding(UTF-8)', 'wtmcsServiceDispatcherDebugLog.yml'))
{
    print F Dump(\%data);
    close(F);
}

foreach my $type (keys %data)
{
    print "> $type\n";

    $db->do("DROP TABLE IF EXISTS [$type];");

    my $table = join(', ', (map { "[$_] $data{$type}->{c}->{$_}->{t}" } sort { $data{$type}->{c}->{$a}->{p} <=> $data{$type}->{c}->{$b}->{p} } keys %{$data{$type}->{c}}));
    $db->do("CREATE TABLE [$type] ($table);");
    foreach my $col (('Status'))
    {
        next unless ($data{$type}->{c}->{$col});
        $db->do("CREATE INDEX [${type}_${col}] ON [$type] ($col);");
    }

    my $inscols = join(', ', sort { $a cmp $b } keys %{$data{$type}->{c}});
    my $insplhs = join(', ', (map { '?' } keys %{$data{$type}->{c}}));
    my $insert = "INSERT INTO [$type] ($inscols) VALUES ($insplhs);";
    my $st = $db->prepare($insert);

    $db->begin_work();
    foreach my $record (@{$data{$type}->{r}})
    {
        my @values = map { !defined($record->{$_}) ? undef : ($data{$type}->{c}->{$_}->{t} ne 'INTEGER' && $data{$type}->{c}->{$_}->{t} ne 'REAL') ? $record->{$_} : ($record->{$_} eq 'False') ? 0 : ($record->{$_} eq 'True') ? 1 : $record->{$_} } sort { $a cmp $b } keys %{$data{$type}->{c}};
        $st->execute(@values);
    }
    $db->commit();
    $st->finish();

    printf("# %u\n", scalar @{$data{$type}->{r}});
}

$db->disconnect();
