#!/usr/bin/env wolframscript

Table[
	paddedAtomicNumber=StringPadLeft[ToString[atomicNumber],3,"0"];
	icon=TemplateApply["Textures\\GUI\\Icons\\``.jpg",{StringPadLeft[ToString[atomicNumber],3,"0"]}];
	iconUrl=Import[TemplateApply["https://periodictable.com/Elements/``/index.html",{paddedAtomicNumber}],{"HTML","ImageLinks"}]//First;
	iconPathJpg=TemplateApply["Textures\\GUI\\Icons\\ingot\\``.jpg",{paddedAtomicNumber}];
	iconPath=TemplateApply["Textures\\GUI\\Icons\\ingot\\``.png",{paddedAtomicNumber}];
	file=URLDownload[iconUrl,iconPathJpg];
	Export[iconPath, Import[file]];
,{atomicNumber,118}]