#!perl

use strict;
use Text::CSV;
use YAML;

my $appdata = $ENV{LOCALAPPDATA};
my $modconf = "$appdata/Colossal Order/Cities_Skylines/ModConfig";

my $modlog = "$modconf/wtmcsServiceDispatcher.log";
my $problemlog = "$modconf/wtmcsServiceDispatcher.ServiceProblemKeeper.log";

my $keepdate = 0;
my $keeptime = 0;
my $keepframe = 0;

sub FilterLog
{
    my $in = undef;
    my $out = undef;

    return undef unless (open($in, '<:encoding(UTF-8)', $modlog));
    die unless (open($out, '>:encoding(UTF-8)', $problemlog));

    print "< $modlog\n";

    while (my $l = <$in>)
    {
        # 2017-05-28 21:08:35.062 Dev:     [wtmcsServiceDispatcher] @12894176 <ServiceProblemKeeper.DevLog> TargetBuidlingProblemWeightingTargetBuilding=42638, Activated sludge process tank; BuildingProblem=108, 1; Modifier=28, 28; Weight
        next unless ($l =~ /^(\S+) (\S+) \S+\s+\[wtmcsServiceDispatcher\] (\@\d+) <ServiceProblemKeeper.DevLog> (.*?)\s*$/m);

        my $d = $1;
        my $t = $2;
        my $f = $3;
        my $m = $4;

        $m =~ s/^(\S+?)((?:Service|Target)Building=)/\[$1\] $2/;
        $m =~ s/^(\[\S+\])/sprintf("%-40s", $1)/e;

        next if ($m =~ /^\[SetServiceBuildingCurrentTargetInfo\].*?; ProblemSize=0$/);

        $m = "$f $m" if ($keepframe);
        $m = "$t $m" if ($keeptime);
        $m = "$d $m" if ($keepdate);

        print $out "$m\n";
    }

    close($out);
    close($in);

    print "> $problemlog\n";

    return 1;
}

if (-e $modlog)
{
    my $mlt = (stat(_))[9];
    my ($plz,$plt) = (-e $problemlog) ? ((stat(_))[7], (stat(_))[9]) : (0, 0);

    if ($plt <= $mlt || !$plz)
    {
        FilterLog();
    }
}
