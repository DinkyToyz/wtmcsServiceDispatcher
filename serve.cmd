@echo off
call bundle exec jekyll build --config _config.yml,_config_dev.yml
call bundle exec jekyll serve --config _config.yml,_config_dev.yml --incremental
