@set sourcedir=%1
@set targetdir=%2

@set s1=%3
@set s2=%4
@set s3=%5
@set s4=%6
@set s5=%7
@set s6=%8


@set tool="%PROGRAMFILES(x86)%\Microsoft SDKs\Windows\v7.0A\Bin\x64\xsd.exe"
@set namespace=TuevSued.V1.IT.NewSyncCommonObjects.DataSets

cd %sourcedir%

for %%x in (%s1% %s2% %s3% %s4%) do (
if not %%x.==. (
%tool% %sourcedir%\%%xDS.xsd /d /out:%targetdir% /n:%namespace%
)
)

