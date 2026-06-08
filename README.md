# HashFinder
A C# console application for automated hash cracking and reverse lookup.

HashFinder is a simple yet powerful tool that helps identify the plaintext value of cryptographic hashes by searching through known hash tables and applying intelligent attack strategies. The application automatically detects hash types (MD5, SHA1, SHA256, SHA512) and attempts to crack them using a multi-stage approach:

- Dictionary attacks with common passwords
- Rule-based mutations to expand password variations
- Hybrid attacks combining wordlists with character patterns
- Intelligent masking with pattern-based brute force
- Intensive brute force as a final resort

### The tool leverages Hashcat for GPU-accelerated cracking and automatically downloads required wordlists and rule files, making it ready to use out of the box, so watch out.

## Key Features:
- Automatic hash type detection
- Multi-strategy attack ordering
- GPU acceleration support for faster and parallel operations
- Self-contained with automatic dependency management
- Licensed under GNU GPL v3.0
