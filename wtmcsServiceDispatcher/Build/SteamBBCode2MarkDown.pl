#!perl

use strict;

sub list
{
    my ($list) = @_;
    $list =~ s|\n\[li\]|\n- |g;
    $list =~ s|\n[ \t]+|\n  |g;
    $list =~ s|(\n- [^\n]*?[\]>])(\n[ \t])|$1: $2|gi;

    return "$list\n";
}

foreach my $file (@ARGV)
{
    next unless (open(F, '<:encoding(UTF-8)', $file));
    my $doc = join('', <F>);
    close(F);

    unless ($file =~ /\.md$/i)
    {
        $doc =~ s/\r\n/\n/;
        $doc =~ s/\r/\n/;

        $doc =~ s|\n\[\*\]|\n[li]|g;
        $doc =~ s|_|\\_|g;
        $doc =~ s|\*|\\*|g;
        $doc =~ s|\n\[h1\]([^\r\n]*?)\[/h1\]\n|\n## $1\n|gi;
        $doc =~ s|\n\[u\]([^\r\n]*?)\[/u\]\n|\n### $1\n|gi;
        $doc =~ s|\n\[list\](\n.*?)\n\[/list\]\n|list($1)|geis;
        $doc =~ s|\[b\](.*?)\[/b\]|**$1**|gis;
        $doc =~ s|\[i\](.*?)\[/i\]|*$1*|gis;
        $doc =~ s|\[url=(.*?)\](.*?)\[/url\]|[$2]($1)|gis;
        $doc =~ s|<([^\n]*?)>|[`$1`]|g;
        $doc =~  s|\[u\](.*?)\[/u\]|<u>$1</u>|gis;
    }

    print $doc;
}
