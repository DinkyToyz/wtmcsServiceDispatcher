---
title: Incompatibilities
sort_order: 700
hide_for_current: true
navigation_menu: I000010
---
{% capture Incompatibilities %}{% include_relative {{ site.current_version }}/_Incompatibilities.md %}{% endcapture %}{{ Incompatibilities | markdownify }}