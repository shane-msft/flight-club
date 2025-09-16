@echo off
REM Launch Neovim with project-specific init.lua safely
C:\Progra~1\Neovim\bin\nvim.exe -u "%~dp0init.lua" %*

