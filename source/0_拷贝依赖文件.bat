@echo off

echo －－－－－ 拷贝 LOGNETDLL －－－－－－－－－－－－－
xcopy /Y /S ..\..\open_src\client\log4net.dll 	LyncWpfApp\LyncWpfApp\Reference\

echo －－－－－ 拷贝 HWUCSDK DLL－－－－－－－－－－－－－
xcopy /Y /S .\..\..\platform\HWUCSDK\windows\eSpace_Desktop_V200R001C50SPC100B091\debug\dll\* 	..\UCService\output\debug\
xcopy /Y /S .\..\..\platform\HWUCSDK\windows\eSpace_Desktop_V200R001C50SPC100B091\release\dll\* ..\UCService\output\release\
echo －－－－－ 拷贝 HWUCSDK DLL 成功－－－－－－－－－－－－

echo －－－－－ 拷贝 UCService依赖的资源文件 －－－－－－－－－－－－－
xcopy /Y /S ..\UCService\UCResource\* 			..\UCService\output\debug\
xcopy /Y /S ..\UCService\UCResource\*  			..\UCService\output\release\
echo －－－－－ 拷贝 UCService依赖的资源文件 成功－－－－－－－－－－－－

pause