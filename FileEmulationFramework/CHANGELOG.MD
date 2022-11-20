# 2.1.1

- Added: Source to start of various log commands.  

# 2.1.0

- Added: `UnregisterVirtualFile` for unregistering files [untested].  

# 2.0.0

- Improved: Performance by a few nanoseconds per read.  
- Added: `RegisterVirtualFile` API for creating files with custom path backed by custom data.  

# 1.1.3

- Changed: Use Thread Local buffer for Symlink Resolution to avoid potential stack overflow.  

# 1.1.0

- Fixed: Emulator will no longer incorrectly run for directories.  
- Added: Support for CloseHandle hook with custom ASM.  

# 1.0.0

Initial Mod Release