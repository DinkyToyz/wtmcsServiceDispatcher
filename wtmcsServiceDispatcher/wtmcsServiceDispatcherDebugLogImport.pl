#!perl

use strict;
use Text::CSV;
use YAML;
use DBI;

my $appdata = $ENV{LOCALAPPDATA};
my $modconf = "$appdata/Colossal Order/Cities_Skylines/ModConfig";

my $db = DBI->connect("dbi:SQLite:dbname=$modconf/wtmcsServiceDispatcherDebugLog.db","","");
$db->do('PRAGMA journal_mode = MEMORY');

my $records = 0;
my %data = ();
my $scsv = Text::CSV->new({sep_char=>';', allow_whitespace=>1, blank_is_undef=>1, escape_char=>'^'});
die unless (open(F,'<:encoding(UTF-8)', "$modconf/wtmcsServiceDispatcher.log"));

print "< wtmcsServiceDispatcher.log\n";
while (my $l = <F>)
{
    next unless ($l =~ /^(\S+ \S+)\s+(?:\S+:\s+)?\[wtmcsServiceDispatcher\]\s+(?:\@(\d+)\s+)?<([A-Za-z]+)\.DebugListLog>\s*(.*?)[\s\r\n]*$/);
    my $stamp =  $1;
    my $frame = $2;
    my $type = $3;
    next unless ($scsv->parse($4));

    $type =~ s/Keeper$//;

    $data{$type} =
    {
        c=>
        {
            _stamp => {p=>-5, t=>'TEXT'},
            _frame => {p=>-4, t=>'INTEGER'},
            _last => {p=>-3, t=>'INTEGER'},
        },
        r=>[]
    } unless ($data{$type});

    my %record =
    (
        _stamp => $stamp,
        _frame => $frame,
        _last => 0,
    );

    my $p = 0;
    foreach my $fld ($scsv->fields)
    {
        next unless ($fld =~ /^([_a-zA-Z0-9]+)=(.*)$/);

        my $col = $1;
        my $val = $2;

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

#if (open(F, '>:encoding(UTF-8)', "$modconf/wtmcsServiceDispatcherDebugLog.yml"))
#{
#    print F Dump(\%data);
#    close(F);
#}

foreach my $type (keys %data)
{
    print "> $type\n";

    $db->do("DROP TABLE IF EXISTS [$type];");

    my $table = join(', ', (map { "[$_] $data{$type}->{c}->{$_}->{t}" } sort { $data{$type}->{c}->{$a}->{p} <=> $data{$type}->{c}->{$b}->{p} } keys %{$data{$type}->{c}}));
    $db->do("CREATE TABLE [$type] ($table);");

    my $key = "${type}Id";
    if (!$data{$type}->{c}->{$key} && $type =~ /^(.*?)s$/)
    {
        $key = "${1}Id";
    }
    if ($data{$type}->{c}->{$key})
    {
        $db->do("CREATE UNIQUE INDEX [${type}_Primary_Index] ON [$type] ([$key], _frame);");
    }

    foreach my $col (('Status'))
    {
        next unless ($data{$type}->{c}->{$col});
        $db->do("CREATE INDEX [${type}_${col}] ON [$type] ([$col]);");
    }

    my $inscols = join(', ', map { "[$_]" } sort { $a cmp $b } keys %{$data{$type}->{c}});
    my $insplhs = join(', ', (map { '?' } keys %{$data{$type}->{c}}));
    my $insert = "INSERT OR REPLACE INTO [$type] ($inscols) VALUES ($insplhs);";
    my $st = $db->prepare($insert);

    $db->begin_work();
    foreach my $record (@{$data{$type}->{r}})
    {
        my @values = map { !defined($record->{$_}) ? undef : ($data{$type}->{c}->{$_}->{t} ne 'INTEGER' && $data{$type}->{c}->{$_}->{t} ne 'REAL') ? $record->{$_} : ($record->{$_} eq 'False') ? 0 : ($record->{$_} eq 'True') ? 1 : $record->{$_} } sort { $a cmp $b } keys %{$data{$type}->{c}};
        $st->execute(@values);
    }
    $db->commit();
    $st->finish();

    if ($key)
    {
        $db->begin_work();
        $db->do("UPDATE [$type] SET _last = 1 WHERE ROWID IN (SELECT ROWID FROM [$type] T1 WHERE NOT EXISTS (SELECT 1 FROM [$type] T2 WHERE T2.[$key] = T1.[$key] AND T2._frame > T1._frame))");
        $db->commit();
    }

    printf("# %u\n", scalar @{$data{$type}->{r}});
}

$db->disconnect();
