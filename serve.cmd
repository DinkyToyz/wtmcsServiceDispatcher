@echo off
call jekyll build --config _config.yml,_config_dev.yml
call jekyll serve --config _config.yml,_config_dev.yml --incremental
