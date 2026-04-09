# IronVault

**IronVault** is a lightweight, high-performance desktop utility built with C# and WPF, designed for secure local file encryption and decryption. It leverages industry-standard cryptographic algorithms to ensure data privacy and protection against unauthorized access.

> **Status:** Ongoing / Minimum Viable Product

## Key Features

* **Advanced Encryption:** Uses **AES-256** (Advanced Encryption Standard) in Cipher Block Chaining (CBC) mode to encrypt file contents, rendering them completely unreadable without the correct password.
* **Robust Key Derivation:** Implements **PBKDF2** (`Rfc2898DeriveBytes`) with a randomized salt and a high iteration count. This significantly slows down dictionary and brute-force attacks.
* **Memory-Optimized Processing:** Utilizes .NET's `CryptoStream` to process files in chunks. This architectural decision allows IronVault to encrypt or decrypt massive files (e.g., gigabytes in size) without exhausting system RAM.
* **Intuitive UI:** Clean and responsive Graphical User Interface built with **WPF** and **XAML**.

## Tech Stack

* **Language:** C#
* **Framework:** .NET / Windows Presentation Foundation (WPF)
* **Core Libraries:** `System.Security.Cryptography`, `System.IO`

## Under the Hood (How it works)

When a user encrypts a file:
1. A unique **Salt** is generated.
2. The user's password and the salt are passed through **PBKDF2** to derive a cryptographically secure 256-bit key and an Initialization Vector (IV).
3. The original file is read sequentially using a `FileStream`.
4. The data passes through a `CryptoStream` where the AES-256 algorithm transforms it into ciphertext.
5. The Salt, IV, and the encrypted data are written to a new `.vault` file. 

*During decryption, the application extracts the Salt and IV from the `.vault` file header to reconstruct the exact cryptographic key needed to reverse the process.*

## Roadmap (Upcoming Features)

As this is an actively developed project, the following features are planned for upcoming releases:
- ✅️ **Asynchronous Processing (`async`/`await`):** To keep the UI fully responsive during the encryption of very large files.
- ✅️ **Progress Bar Integration:** Visual feedback mapping the `IProgress` of the `CryptoStream` operations.
- ✅️ **Drag & Drop Support:** Allow users to drag files directly from Windows Explorer into the application window.
- ✅️ **Password Strength Meter:** Real-time visual feedback on password complexity.
- [ ] **Password "Eye" Button:** Allows users to see the passwrod they're typing if they want to.
- [ ] **File deletion CheckBox:** ALlows users to delete the file they just encrypted from the source if they want to.

## How to Run locally
1. Clone the repository: `git clone https://github.com/IonitaVlad31/IronVault.git`
2. Open the solution `.sln` file in **Visual Studio**.
3. Build and Run (F5).
