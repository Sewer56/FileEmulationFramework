; Params
; {0} = address of list from C# code.
use64

; Stub
push rcx
mov rdx, rcx ; handle ptr
mov rcx, {0} ; list
sub rsp, 32
call AddItem_IG01
add rsp, 32
pop rcx
jmp exit

AddItem_IG01:
push rbp
push r14
push rdi
push rsi
push rbx
sub rsp, 32
lea rbp, [rsp+40H]
mov rsi, rcx
mov rdi, rdx
 
AddItem_IG02:
mov qword [rbp+10H], rsi
mov rax, qword [rsi+10H]
 
AddItem_IG03:
call rax
mov esi, eax
mov rbx, qword [rbp+10H]
lea r14, qword [rbx+28H]
 
AddItem_IG04:
mov eax, -1
lock 
cmpxchg dword [r14], esi
cmp eax, -1
jne SHORT AddItem_IG04
 
AddItem_IG05:
mov ecx, dword [rbx+20H]
cmp ecx, dword [rbx+24H]
jl SHORT AddItem_IG07
 
AddItem_IG06:
mov rcx, rbx
call Grow_IG01
 
AddItem_IG07:
mov rax, qword [rbx+18H]
mov edx, dword [rbx+20H]
lea ecx, [rdx+01H]
mov dword [rbx+20H], ecx
movsxd rdx, edx
mov qword [rax+8*rdx], rdi
mov dword [rbx+28H], -1
 
AddItem_IG08:
add rsp, 32
pop rbx
pop rsi
pop rdi
pop r14
pop rbp
ret

Grow_IG01: 
push rbp
push r14
push rdi
push rsi
push rbx
sub rsp, 32
lea rbp, [rsp+40H]
mov rsi, rcx
 
Grow_IG02: 
mov ecx, dword [rsi+24H]
mov edi, ecx
mov qword [rbp+10H], rsi
mov rax, qword [rsi]
add ecx, ecx
shl ecx, 3
movsxd rcx, ecx
 
Grow_IG03: 
call rax
mov rsi, rax
xor ebx, ebx
 
Grow_IG04: 
test edi, edi
jle SHORT Grow_IG10
align 16
 
Grow_IG05: 
mov r14, qword [rbp+10H]
mov rcx, qword [r14+18H]
movsxd rax, ebx
mov rcx, qword [rcx+8*rax]
mov qword [rsi+8*rax], rcx
inc ebx
cmp ebx, edi
jl SHORT Grow_IG09
 
Grow_IG06: 
mov rax, qword [r14+08H]
mov qword [rbp+10H], r14
mov rcx, qword [r14+18H]
 
Grow_IG07: 
call rax
mov r14, qword [rbp+10H]
mov qword [r14+18H], rsi
shl dword [r14+24H], 1
 
Grow_IG08: 
add rsp, 32
pop rbx
pop rsi
pop rdi
pop r14
pop rbp
ret 
 
Grow_IG09: 
mov qword [rbp+10H], r14
jmp SHORT Grow_IG05
 
Grow_IG10: 
mov r14, qword [rbp+10H]
jmp SHORT Grow_IG06

exit:
