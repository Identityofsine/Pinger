REM delete the old zip
del pkg.zip

REM zip all the contents of the pkg folder
cd pkg
7z a -tzip ..\pkg.zip *

