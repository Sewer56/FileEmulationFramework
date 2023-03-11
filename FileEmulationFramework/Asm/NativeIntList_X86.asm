use32

; during this call we have handle on stack, we need to call 
mov ecx, dword {0}      ; list instance
mov edx, dword [esp+4h] ; re-push handle
call AddItem_IG01 ; <= clrcall
jmp exit

AddItem_IG01: 
push ebp
mov ebp, esp
push edi
push esi
push ebx
sub esp, 8
mov esi, ecx
mov edi, edx

AddItem_IG02: 
mov dword [ebp-10H], esi
mov eax, dword [esi+08H]

AddItem_IG03: 
call eax
mov esi, eax
mov ebx, dword [ebp-10H]
lea edx, dword [ebx+18H]
mov dword [ebp-14H], edx

AddItem_IG04: 
mov eax, -1
lock cmpxchg dword [edx], esi
cmp eax, -1
jne SHORT AddItem_IG04

AddItem_IG05: 
mov ecx, dword [ebx+10H]
cmp ecx, dword [ebx+14H]
jl SHORT AddItem_IG07

AddItem_IG06: 
mov ecx, ebx
call Grow_IG01

AddItem_IG07: 
mov eax, dword [ebx+0CH]
mov edx, dword [ebx+10H]
lea ecx, [edx+01H]
mov dword [ebx+10H], ecx
mov dword [eax+4*edx], edi
mov dword [ebx+18H], -1

AddItem_IG08: 
lea esp, [ebp-0CH]
pop ebx
pop esi
pop edi
pop ebp
ret

Grow_IG01:
push ebp
mov ebp, esp
push edi
push esi
push ebx
push eax
mov esi, ecx

Grow_IG02: 
mov eax, dword [esi+14H]
mov edi, eax
mov dword [ebp-10H], esi
mov edx, dword [esi]
add eax, eax
shl eax, 2
push eax

Grow_IG03: 
call edx
pop ecx
mov esi, eax
xor ebx, ebx

Grow_IG04: 
test edi, edi
jle SHORT Grow_IG10
align 16

Grow_IG05: 
mov eax, dword [ebp-10H]
mov edx, dword [eax+0CH]
mov edx, dword [edx+4*ebx]
mov dword [esi+4*ebx], edx
inc ebx
cmp ebx, edi
jl SHORT Grow_IG09

Grow_IG06: 
mov edx, dword [eax+04H]
mov dword [ebp-10H], eax
push dword [eax+0CH]

Grow_IG07: 
call edx
pop ecx
mov edi, dword [ebp-10H]
mov dword [edi+0CH], esi
shl dword [edi+14H], 1

Grow_IG08: 
pop ecx
pop ebx
pop esi
pop edi
pop ebp
ret

Grow_IG09: 
mov dword [ebp-10H], eax
jmp SHORT Grow_IG05

Grow_IG10: 
mov eax, dword [ebp-10H]
jmp SHORT Grow_IG06

exit: