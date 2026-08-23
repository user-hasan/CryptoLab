using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoLab.App.Services;

internal static class PythonContent
{
    public static string Code(string algorithmId) => algorithmId.ToLowerInvariant() switch
    {
        "atbash" => AtbashCode,
        "rot13" => Rot13Code,
        "vigenere" => VigenereCode,
        "affine" => AffineCode,
        "rail-fence" => RailFenceCode,
        "columnar" => ColumnarCode,
        "aes" => AesCode,
        "des" => DesCode,
        "3des" => TdesCode,
        "rsa" => RsaCode,
        "md5" => Md5Code,
        "sha1" => Sha1Code,
        "sha256" => Sha256Code,
        "sha512" => Sha512Code,
        _ => CaesarCode
    };

    public static string Intro(string algorithmId, bool arabic) => Pick(IntrosEn, IntrosAr, algorithmId, arabic);

    public static string Libraries(string algorithmId, bool arabic) => Pick(LibrariesEn, LibrariesAr, algorithmId, arabic);

    public static IReadOnlyList<string> Functions(string algorithmId, bool arabic)
    {
        if (!FunctionsByAlgorithm.TryGetValue(algorithmId, out var entries)) return Array.Empty<string>();
        return entries.Select(e => e.Name + " — " + (arabic ? e.Ar : e.En)).ToArray();
    }

    private static string Pick(Dictionary<string, string> en, Dictionary<string, string> ar, string id, bool arabic)
    {
        var source = arabic ? ar : en;
        if (source.TryGetValue(id, out var value)) return value;
        return en.TryGetValue(id, out var fallback) ? fallback : string.Empty;
    }

    private static readonly Dictionary<string, string> IntrosEn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["caesar"] = "A pure-Python Caesar cipher: every Latin letter is shifted by a fixed number of positions in the alphabet. Non-letter characters pass through unchanged and letter case is preserved.",
        ["atbash"] = "A pure-Python Atbash: each letter is mirrored across the alphabet (A<->Z, B<->Y, ...). The operation is symmetric, so encryption and decryption are the same call.",
        ["rot13"] = "ROT13 rotates every Latin letter by 13 positions. Because 13 + 13 = 26, applying it twice returns the original text, so encryption and decryption share one function.",
        ["vigenere"] = "Vigenere over the Latin alphabet: the keyword repeats over the text, and each of its letters selects a different Caesar shift for every plaintext letter.",
        ["affine"] = "Affine cipher E(x) = (a*x + b) mod 26 with the requirement gcd(a, 26) = 1 so that decryption is possible. The modular inverse of a is computed with the extended Euclid algorithm.",
        ["rail-fence"] = "Rail Fence writes the text in a zigzag pattern across `rails` rows, then reads it row by row. Decryption rebuilds the pattern and refills the rows with the ciphertext.",
        ["columnar"] = "Columnar transposition fills a grid row by row, then reads the columns in the alphabetical order of the keyword letters. Decryption reverses that order.",
        ["aes"] = "AES-128 from scratch: the S-box is generated mathematically from the GF(2^8) inverse plus an affine transform (no lookup table copied), the key schedule builds 11 round keys, and 10 rounds are applied. CBC mode with PKCS7 padding chains the blocks together. Only bytes are handled, never text.",
        ["des"] = "DES from scratch using the official FIPS 46-3 tables (IP, FP, E, P, PC-1, PC-2 and the eight S-boxes). The 64-bit key produces 16 subkeys of 48 bits; a 16-round Feistel network transforms each 64-bit block, and CBC mode with PKCS7 padding handles whole messages.",
        ["3des"] = "3DES reuses the DES module: Encrypt = E(k3, D(k2, E(k1, P))) and Decrypt is the exact reverse. A 16-byte key gives (k1, k2, k1); a 24-byte key gives three distinct keys. CBC mode with PKCS7 padding.",
        ["rsa"] = "RSA from scratch: the Miller-Rabin test generates two large primes; n = p*q, e = 65537 and d = e^-1 mod phi(n). Encryption raises each block to e mod n, decryption to d mod n. Every block stores its own byte length as a prefix so the original message is restored exactly.",
        ["md5"] = "MD5 from scratch with the Merkle-Damgard construction: the message is padded (0x80 byte, zeros, then the 64-bit length with the low word first), processed in 512-bit blocks through 64 rounds with four shift schedules, producing a 128-bit digest.",
        ["sha1"] = "SHA-1 from scratch with the Merkle-Damgard construction: 512-bit blocks, 80 rounds with five distinct constants, producing a 160-bit digest. The 64-bit length is appended big-endian.",
        ["sha256"] = "SHA-256 from scratch: the eight initial hash values and the 64 round constants are derived from the fractional parts of the square/cube roots of the first primes, computed with high-precision decimal arithmetic so every bit is exact. 512-bit blocks, 64 rounds, 256-bit digest.",
        ["sha512"] = "SHA-512 from scratch with 64-bit words: the eight initial values and the 80 round constants are derived from the fractional parts of the square/cube roots of the first primes via high-precision decimal arithmetic. 1024-bit blocks, 80 rounds, 512-bit digest."
    };

    private static readonly Dictionary<string, string> IntrosAr = new(StringComparer.OrdinalIgnoreCase)
    {
        ["caesar"] = "تنفيذ بايثون خالص لشيفرة قيصر: تُزاح كل حرف لاتيني عددًا ثابتًا من المواضع داخل الأبجدية. تُترك الرموز غير الحرفية كما هي مع الحفاظ على حالة الأحرف.",
        ["atbash"] = "تنفيذ بايثون خالص لشيفرة أتباش: يُعكس كل حرف عبر الأبجدية (A↔Z، B↔Y، ...). العملية متماثلة، لذا التشفير وفك التشفير هما الاستدعاء نفسه.",
        ["rot13"] = "يدوّر ROT13 كل حرف لاتيني 13 موضعًا. ولأن 13 + 13 = 26، فإن تطبيقه مرتين يعيد النص الأصلي، لذا يشترك التشفير وفك التشفير في دالة واحدة.",
        ["vigenere"] = "فيجنير على الأبجدية اللاتينية: تتكرر الكلمة المفتاحية على النص، ويختار كل حرف منها إزاحة مختلفة لكل حرف من النص الأصلي.",
        ["affine"] = "شيفرة Affine بالصيغة E(x) = (a·x + b) mod 26 مع اشتراط gcd(a, 26) = 1 حتى يكون فك التشفير ممكنًا. يُحسب المعكوس المعياري لـ a بخوارزمية إقليدس الموسعة.",
        ["rail-fence"] = "يكتب Rail Fence النص بنمط متعرج عبر عدد من المسارات ثم يقرؤه مسارًا مسارًا. فك التشفير يعيد بناء النمط ويملأ المسارات بأحرف النص المشفر.",
        ["columnar"] = "التبديل العمودي يملأ شبكة سطرًا سطرًا ثم يقرأ الأعمدة بالترتيب الأبجدي لحروف الكلمة المفتاحية. فك التشفير يعكس هذا الترتيب.",
        ["aes"] = "AES-128 من الصفر: يُولَّد S-box رياضيًا من المعكوس في حقل GF(2^8) مع تحويل أفيني (دون نسخ جدول جاهز)، وتولد خوارزمية توسيع المفتاح 11 مفتاح جولة، وتُطبَّق 10 جولات. وضع CBC مع حشو PKCS7 يربط الكتل معًا. تُعالج البايتات فقط وليس النص.",
        ["des"] = "DES من الصفر باستخدام جداول FIPS 46-3 الرسمية (IP وFP وE وP وPC-1 وPC-2 وصناديق S الثمانية). يولّد مفتاح 64 بت 16 مفتاحًا فرعيًا من 48 بتًا، وتُحوِّل شبكة Feistel ذات الـ16 جولة كل كتلة 64 بت، ويعالج وضع CBC مع حشو PKCS7 الرسائل الكاملة.",
        ["3des"] = "يعيد 3DES استخدام وحدة DES: التشفير = E(k3, D(k2, E(k1, P))) وفك التشفير بالعكس تمامًا. مفتاح 16 بايت يعطي (k1, k2, k1)، ومفتاح 24 بايت يعطي ثلاثة مفاتيح مختلفة. وضع CBC مع حشو PKCS7.",
        ["rsa"] = "RSA من الصفر: يختبر اختبار ميلر-رابين أولية عددين كبيرين؛ n = p·q وe = 65537 وd = e⁻¹ mod φ(n). يرفع التشفير كل كتلة إلى e معامل n، وفك التشفير إلى d معامل n. تحمل كل كتلة طولها بالبايت كبادئة لاستعادة الرسالة الأصلية بدقة.",
        ["md5"] = "MD5 من الصفر ببنية ميركل-دامغارد: تُحشى الرسالة (بايت 0x80 ثم أصفار ثم الطول 64 بت مع الكلمة الواطئة أولًا)، وتُعالج في كتل 512 بت عبر 64 جولة بأربعة جداول إزاحات، وينتج ملخص 128 بت.",
        ["sha1"] = "SHA-1 من الصفر ببنية ميركل-دامغارد: كتل 512 بت و80 جولة بخمس ثوابت مختلفة وينتج ملخص 160 بت. يُلحق الطول 64 بت بترتيب كبير الأهمية أولًا.",
        ["sha256"] = "SHA-256 من الصفر: تُشتق قيم الهاش الثماني الأولية وثوابت الجولات الـ64 من الأجزاء الكسرية للجذور التربيعية/التكعيبية للأعداد الأولية الأولى، بحساب عشري عالي الدقة لتكون كل البتات دقيقة. كتل 512 بت و64 جولة وملخص 256 بت.",
        ["sha512"] = "SHA-512 من الصفر بكلمات 64 بت: تُشتق القيم الثماني الأولية وثوابت الجولات الـ80 من الأجزاء الكسرية للجذور التربيعية/التكعيبية للأعداد الأولية الأولى بحساب عشري عالي الدقة. كتل 1024 بت و80 جولة وملخص 512 بت."
    };

    private static readonly Dictionary<string, string> LibrariesEn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["caesar"] = "string — uppercase alphabet constant",
        ["atbash"] = "None (pure Python)",
        ["rot13"] = "None (pure Python)",
        ["vigenere"] = "None (pure Python)",
        ["affine"] = "None (pure Python)",
        ["rail-fence"] = "None (pure Python)",
        ["columnar"] = "None (pure Python)",
        ["aes"] = "None (pure Python)",
        ["des"] = "None (pure Python)",
        ["3des"] = "des — the local DES module",
        ["rsa"] = "random — random number source for prime generation",
        ["md5"] = "math — round constants derived from sin(i)",
        ["sha1"] = "None (pure Python)",
        ["sha256"] = "decimal — high-precision constants (80 digits)",
        ["sha512"] = "decimal — high-precision constants (80 digits)"
    };

    private static readonly Dictionary<string, string> LibrariesAr = new(StringComparer.OrdinalIgnoreCase)
    {
        ["caesar"] = "string — ثابت الأبجدية الكبيرة",
        ["atbash"] = "لا شيء (بايثون خالص)",
        ["rot13"] = "لا شيء (بايثون خالص)",
        ["vigenere"] = "لا شيء (بايثون خالص)",
        ["affine"] = "لا شيء (بايثون خالص)",
        ["rail-fence"] = "لا شيء (بايثون خالص)",
        ["columnar"] = "لا شيء (بايثون خالص)",
        ["aes"] = "لا شيء (بايثون خالص)",
        ["des"] = "لا شيء (بايثون خالص)",
        ["3des"] = "des — وحدة DES المحلية",
        ["rsa"] = "random — مصدر الأعداد العشوائية لتوليد الأعداد الأولية",
        ["md5"] = "math — ثوابت الجولات المشتقة من sin(i)",
        ["sha1"] = "لا شيء (بايثون خالص)",
        ["sha256"] = "decimal — ثوابت عالية الدقة (80 خانة)",
        ["sha512"] = "decimal — ثوابت عالية الدقة (80 خانة)"
    };

    private static (string Name, string En, string Ar) F(string name, string en, string ar) => (name, en, ar);

    private static readonly Dictionary<string, (string Name, string En, string Ar)[]> FunctionsByAlgorithm = new(StringComparer.OrdinalIgnoreCase)
    {
        ["caesar"] = new[]
        {
            F("shift_char", "Shifts a single letter by `shift` positions, preserving its case.",
              "تُزاح حرفًا واحدًا بمقدار `shift` مع الحفاظ على حالته."),
            F("encrypt", "Moves every letter forward by `shift` positions.",
              "تُحرّك كل حرف إلى الأمام بمقدار `shift`."),
            F("decrypt", "Moves every letter backward by `shift` positions.",
              "تُرجع كل حرف إلى الخلف بمقدار `shift`.")
        },
        ["atbash"] = new[]
        {
            F("encrypt", "Mirrors every letter across the alphabet (A->Z, B->Y, ...).",
              "تعكس كل حرف عبر الأبجدية (A→Z، B→Y، ...)."),
            F("decrypt", "Identical to encrypt, because Atbash is symmetric.",
              "مطابقة للتشفير لأن أتباش متماثلة.")
        },
        ["rot13"] = new[]
        {
            F("rot13", "Rotates every letter by 13 positions, preserving case.",
              "تدوّر كل حرف 13 موضعًا مع الحفاظ على حالته."),
            F("encrypt", "Calls rot13; encryption is the 13-position rotation.",
              "تستدعي rot13؛ التشفير هو التدوير بمقدار 13."),
            F("decrypt", "Calls rot13; the same rotation is its own inverse.",
              "تستدعي rot13؛ التدوير نفسه هو معكوسه.")
        },
        ["vigenere"] = new[]
        {
            F("letter_value", "Converts a letter to its numeric value: A -> 0 ... Z -> 25.",
              "تحوّل الحرف إلى قيمته الرقمية: A=0 ... Z=25."),
            F("shift_letter", "Moves one letter by a shift, preserving its case.",
              "تُزاح حرفًا واحدًا بمقدار إزاحة مع الحفاظ على حالته."),
            F("encrypt", "Applies the repeating keyword shifts forward over the text.",
              "تطبق إزاحات الكلمة المفتاحية المتكررة للأمام على النص."),
            F("decrypt", "Applies the same keyword shifts backwards.",
              "تطبق إزاحات الكلمة المفتاحية نفسها للخلف.")
        },
        ["affine"] = new[]
        {
            F("gcd", "Greatest common divisor with Euclid's algorithm.",
              "القاسم المشترك الأكبر بخوارزمية إقليدس."),
            F("mod_inverse", "Multiplicative inverse of a modulo m (extended Euclid).",
              "المعكوس الضربي لـ a معامل m (إقليدس الموسعة)."),
            F("transform_char", "Applies E(x) = (a*x + b) mod 26 or its inverse to one letter.",
              "تطبق E(x) = (a·x + b) mod 26 أو معكوسها على حرف واحد."),
            F("encrypt", "Encrypts text; asserts gcd(a, 26) = 1 first.",
              "تشفر النص بعد التحقق من gcd(a, 26) = 1."),
            F("decrypt", "Decrypts with D(y) = a^-1 * (y - b) mod 26.",
              "تفك التشفير بالصيغة D(y) = a⁻¹·(y − b) mod 26.")
        },
        ["rail-fence"] = new[]
        {
            F("zigzag_pattern", "Returns the rail index of every character position (0-based).",
              "تعيد مؤشر المسار لكل موضع حرف (يبدأ من صفر)."),
            F("encrypt", "Writes the text in zigzag order, then reads it rail by rail.",
              "تكتب النص بنمط متعرج ثم تقرؤه مسارًا مسارًا."),
            F("decrypt", "Rebuilds the zigzag positions and refills them with ciphertext.",
              "تعيد بناء المواضع المتعرجة وتملؤها بالنص المشفر.")
        },
        ["columnar"] = new[]
        {
            F("column_order", "Column indexes sorted by the alphabetical order of the keyword.",
              "مؤشرات الأعمدة مرتبة حسب الترتيب الأبجدي للكلمة المفتاحية."),
            F("encrypt", "Fills the grid row by row, then reads columns in keyword order.",
              "تملأ الشبكة سطرًا سطرًا ثم تقرأ الأعمدة بترتيب المفتاح."),
            F("decrypt", "Puts the ciphertext back into the columns, then reads row by row.",
              "تعيد النص المشفر إلى الأعمدة ثم تقرأ سطرًا سطرًا.")
        },
        ["aes"] = new[]
        {
            F("gf_mult", "Multiplies two bytes in GF(2^8) with the AES polynomial x^8+x^4+x^3+x+1.",
              "تضرب بايتين في حقل GF(2^8) بكثيرة حدود AES."),
            F("gf_pow", "Exponentiation in GF(2^8) by square-and-multiply; x^254 gives the multiplicative inverse.",
              "الرفع في GF(2^8) بطريقة التربيع والضرب؛ x^254 يعطي المعكوس الضربي."),
            F("rotl8", "Rotates an 8-bit value left by `bits` positions.",
              "تدوّر قيمة 8 بت لليسار بمقدار `bits`."),
            F("build_tables", "Generates the S-box (GF inverse + affine transform) and its inverse mathematically.",
              "تولّد S-box (معكوس GF + تحويل أفيني) ومعكوسه رياضيًا."),
            F("key_expansion", "Expands a 16-byte key into 11 round keys of 16 bytes (RotWord, SubWord, Rcon).",
              "توسّع مفتاح 16 بايت إلى 11 مفتاح جولة (RotWord وSubWord وRcon)."),
            F("state_from_block", "Converts 16 bytes into the 4x4 state matrix.",
              "تحوّل 16 بايت إلى مصفوفة الحالة 4×4."),
            F("block_from_state", "Converts the 4x4 state matrix back into 16 bytes.",
              "تحوّل مصفوفة الحالة 4×4 إلى 16 بايت."),
            F("add_round_key", "XORs the state with one 16-byte round key.",
              "تجمع الحالة مع مفتاح جولة 16 بايت بإكسور."),
            F("sub_bytes", "Replaces every byte with its S-box value.",
              "تستبدل كل بايت بقيمة S-box."),
            F("inv_sub_bytes", "Replaces every byte with its inverse S-box value.",
              "تستبدل كل بايت بقيمة معكوس S-box."),
            F("shift_rows", "Rotates row i of the state left by i positions.",
              "تدوّر الصف i من الحالة لليسار بمقدار i."),
            F("inv_shift_rows", "Rotates row i of the state right by i positions.",
              "تدوّر الصف i من الحالة لليمين بمقدار i."),
            F("mix_columns", "Multiplies every column by the fixed AES matrix in GF(2^8).",
              "تضرب كل عمود بمصفوفة AES الثابتة في GF(2^8)."),
            F("inv_mix_columns", "Multiplies every column by the inverse AES matrix.",
              "تضرب كل عمود بالمصفوفة المعكوسة."),
            F("encrypt_block", "Encrypts one 16-byte block through the 10 AES rounds.",
              "تشفر كتلة 16 بايت عبر جولات AES العشر."),
            F("decrypt_block", "Decrypts one block with the inverse operations.",
              "تفك تشفير كتلة واحدة بالعمليات العكسية."),
            F("pkcs7_pad", "Pads the data to a multiple of 16 bytes.",
              "تحشو البيانات حتى مضاعف 16 بايت."),
            F("pkcs7_unpad", "Removes the PKCS7 padding.",
              "تزيل حشو PKCS7."),
            F("encrypt_cbc", "CBC encryption: each block is XORed with the previous ciphertext block.",
              "تشفير CBC: تُجمع كل كتلة بإكسور مع كتلة النص المشفر السابقة."),
            F("decrypt_cbc", "CBC decryption in reverse order, then removes the padding.",
              "فك تشفير CBC بالترتيب المعاكس ثم إزالة الحشو.")
        },
        ["des"] = new[]
        {
            F("permute", "Reorders a bit list using a 1-based permutation table.",
              "تعيد ترتيب قائمة البتات باستخدام جدول تبديل يبدأ من 1."),
            F("bytes_to_bits", "Converts bytes into a flat list of 0/1 bits.",
              "تحوّل البايتات إلى قائمة بتات 0/1."),
            F("bits_to_bytes", "Converts a flat bit list back into bytes.",
              "تعيد تحويل قائمة البتات إلى بايتات."),
            F("generate_subkeys", "Derives the 16 subkeys of 48 bits from the 64-bit key (PC-1, shifts, PC-2).",
              "تشتق 16 مفتاحًا فرعيًا من 48 بتًا من مفتاح 64 بت (PC-1 والإزاحات وPC-2)."),
            F("feistel_function", "F(R, K): expansion, key XOR, S-box substitution, then P permutation.",
              "F(R, K): توسيع ثم إكسور بالمفتاح ثم استبدال بصناديق S ثم تبديل P."),
            F("des_encrypt_block", "Encrypts one 64-bit block through the 16 Feistel rounds.",
              "تشفر كتلة 64 بت عبر جولات Feistel الـ16."),
            F("des_decrypt_block", "Decrypts one block using the subkeys in reverse order.",
              "تفك تشفير كتلة واحدة بالمفاتيح الفرعية بترتيب معكوس."),
            F("pkcs7_pad", "Pads the data to a multiple of 8 bytes (DES block size).",
              "تحشو البيانات حتى مضاعف 8 بايت (حجم كتلة DES)."),
            F("pkcs7_unpad", "Removes the PKCS7 padding.",
              "تزيل حشو PKCS7."),
            F("encrypt_cbc", "CBC encryption: each 8-byte block is XORed with the previous ciphertext.",
              "تشفير CBC: تُجمع كل كتلة 8 بايت بإكسور مع النص المشفر السابق."),
            F("decrypt_cbc", "CBC decryption, then removes the padding.",
              "فك تشفير CBC ثم إزالة الحشو.")
        },
        ["3des"] = new[]
        {
            F("split_keys", "Splits a 16-byte key into (k1, k2, k1) or a 24-byte key into (k1, k2, k3).",
              "تقسم مفتاح 16 بايت إلى (k1, k2, k1) أو مفتاح 24 بايت إلى (k1, k2, k3)."),
            F("encrypt_block", "Encrypts one 8-byte block with three DES passes: E(k3, D(k2, E(k1, P))).",
              "تشفر كتلة 8 بايت بثلاث مرات DES: E(k3, D(k2, E(k1, P)))."),
            F("decrypt_block", "Decrypts one block with the three passes in reverse order.",
              "تفك تشفير كتلة واحدة بالمرات الثلاث بالترتيب المعاكس."),
            F("pkcs7_pad", "Pads the data to a multiple of 8 bytes.",
              "تحشو البيانات حتى مضاعف 8 بايت."),
            F("pkcs7_unpad", "Removes the PKCS7 padding.",
              "تزيل حشو PKCS7."),
            F("encrypt_cbc", "CBC encryption over 8-byte blocks.",
              "تشفير CBC عبر كتل 8 بايت."),
            F("decrypt_cbc", "CBC decryption, then removes the padding.",
              "فك تشفير CBC ثم إزالة الحشو.")
        },
        ["rsa"] = new[]
        {
            F("gcd", "Greatest common divisor with Euclid's algorithm.",
              "القاسم المشترك الأكبر بخوارزمية إقليدس."),
            F("extended_gcd", "Returns (g, x, y) with g = gcd(a, b) and a*x + b*y = g.",
              "تعيد (g, x, y) بحيث g = gcd(a, b) وa·x + b·y = g."),
            F("mod_inverse", "Multiplicative inverse of a modulo m.",
              "المعكوس الضربي لـ a معامل m."),
            F("mod_exp", "Fast modular exponentiation (square-and-multiply).",
              "الرفع المعياري السريع (التربيع والضرب)."),
            F("is_prime", "Miller-Rabin primality test; rounds control the confidence level.",
              "اختبار ميلر-رابين للأولية؛ عدد الجولات يحدد مستوى الثقة."),
            F("generate_prime", "Generates a probable prime with the given bit length.",
              "تولّد عددًا أوليًا محتملًا بطول بتات محدد."),
            F("generate_keypair", "Generates the public key (e, n) and private key (d, n).",
              "تولّد المفتاح العام (e, n) والمفتاح الخاص (d, n)."),
            F("encrypt", "c = m^e mod n block by block, each block prefixed with its byte length.",
              "c = mᵉ mod n كتلة كتلة، مع بادئة طول لكل كتلة."),
            F("decrypt", "m = c^d mod n for every block, then restores the original bytes.",
              "m = cᵈ mod n لكل كتلة ثم تعيد البايتات الأصلية.")
        },
        ["md5"] = new[]
        {
            F("left_rotate", "Rotates a 32-bit word left by `amount` positions.",
              "تدوّر كلمة 32 بت لليسار بمقدار `amount`."),
            F("pad_message", "Appends 0x80, zeros, and the 64-bit length with the low word first.",
              "تلحق 0x80 ثم أصفارًا ثم الطول 64 بت مع الكلمة الواطئة أولًا."),
            F("md5", "Computes the 128-bit digest over 512-bit blocks with 64 rounds.",
              "تحسب الملخص 128 بت عبر كتل 512 بت و64 جولة."),
            F("hexdigest", "Returns the digest as a lowercase hexadecimal string.",
              "تعيد الملخص بصيغة سداسية عشرية صغيرة.")
        },
        ["sha1"] = new[]
        {
            F("left_rotate", "Rotates a 32-bit word left by `amount` positions.",
              "تدوّر كلمة 32 بت لليسار بمقدار `amount`."),
            F("pad_message", "Appends 0x80, zeros, and the 64-bit big-endian length.",
              "تلحق 0x80 ثم أصفارًا ثم الطول 64 بت كبير الأهمية أولًا."),
            F("sha1", "Computes the 160-bit digest over 512-bit blocks with 80 rounds.",
              "تحسب الملخص 160 بت عبر كتل 512 بت و80 جولة."),
            F("hexdigest", "Returns the digest as a lowercase hexadecimal string.",
              "تعيد الملخص بصيغة سداسية عشرية صغيرة.")
        },
        ["sha256"] = new[]
        {
            F("right_rotate", "Rotates a 32-bit word right by `amount` positions.",
              "تدوّر كلمة 32 بت لليمين بمقدار `amount`."),
            F("first_primes", "Returns the first `count` prime numbers (sieve of Eratosthenes).",
              "تعيد أول `count` عددًا أوليًا (منخل إراتوستينس)."),
            F("frac_bits", "Extracts the first `bits` bits of the fractional part of a Decimal root.",
              "تستخرج أول `bits` بتًا من الجزء الكسري لجذر عشري."),
            F("build_constants", "Builds the initial hash values H and the 64 round constants K.",
              "تبني قيم الهاش الأولية H وثوابت الجولات الـ64 K."),
            F("pad_message", "Appends 0x80, zeros, and the 64-bit big-endian length.",
              "تلحق 0x80 ثم أصفارًا ثم الطول 64 بت كبير الأهمية أولًا."),
            F("sha256", "Computes the 256-bit digest over 512-bit blocks with 64 rounds.",
              "تحسب الملخص 256 بت عبر كتل 512 بت و64 جولة."),
            F("hexdigest", "Returns the digest as a lowercase hexadecimal string.",
              "تعيد الملخص بصيغة سداسية عشرية صغيرة.")
        },
        ["sha512"] = new[]
        {
            F("right_rotate", "Rotates a 64-bit word right by `amount` positions.",
              "تدوّر كلمة 64 بت لليمين بمقدار `amount`."),
            F("first_primes", "Returns the first `count` prime numbers (sieve of Eratosthenes).",
              "تعيد أول `count` عددًا أوليًا (منخل إراتوستينس)."),
            F("frac_bits", "Extracts the first `bits` bits of the fractional part of a Decimal root.",
              "تستخرج أول `bits` بتًا من الجزء الكسري لجذر عشري."),
            F("build_constants", "Builds the initial hash values H and the 80 round constants K.",
              "تبني قيم الهاش الأولية H وثوابت الجولات الـ80 K."),
            F("pad_message", "Appends 0x80, zeros, and the 128-bit big-endian length.",
              "تلحق 0x80 ثم أصفارًا ثم الطول 128 بت كبير الأهمية أولًا."),
            F("sha512", "Computes the 512-bit digest over 1024-bit blocks with 80 rounds.",
              "تحسب الملخص 512 بت عبر كتل 1024 بت و80 جولة."),
            F("hexdigest", "Returns the digest as a lowercase hexadecimal string.",
              "تعيد الملخص بصيغة سداسية عشرية صغيرة.")
        }
    };

    private const string CaesarCode = """"
        """Caesar Cipher: every letter is shifted by a fixed number of positions."""

        import string

        ALPHABET = string.ascii_uppercase


        def shift_char(char, shift):
            """Shift one letter by `shift` positions, preserving its case."""
            if "A" <= char.upper() <= "Z":
                index = ALPHABET.index(char.upper())
                shifted = ALPHABET[(index + shift) % 26]
                return shifted if char.isupper() else shifted.lower()
            return char


        def encrypt(text, shift):
            """Encrypt: move every letter forward by `shift` positions."""
            return "".join(shift_char(c, shift) for c in text)


        def decrypt(text, shift):
            """Decrypt: move every letter backward by `shift` positions."""
            return "".join(shift_char(c, -shift) for c in text)
        """";

    private const string AtbashCode = """"
        """Atbash: monoalphabetic substitution that mirrors the alphabet (A<->Z)."""


        def encrypt(text):
            """Mirror the alphabet: A->Z, B->Y, C->X, ... preserving case."""
            result = []
            for char in text:
                if "A" <= char.upper() <= "Z":
                    index = ord(char.upper()) - ord("A")
                    mirrored = chr(ord("Z") - index)
                    result.append(mirrored if char.isupper() else mirrored.lower())
                else:
                    result.append(char)
            return "".join(result)


        def decrypt(text):
            """Atbash is symmetric: decrypting is exactly the same operation."""
            return encrypt(text)
        """";

    private const string Rot13Code = """"
        """ROT13: fixed Caesar shift of 13; its own inverse because 13*2 = 26."""


        def rot13(text):
            """Rotate every letter by 13 positions, preserving case."""
            result = []
            for char in text:
                if "A" <= char.upper() <= "Z":
                    index = (ord(char.upper()) - ord("A") + 13) % 26
                    shifted = chr(ord("A") + index)
                    result.append(shifted if char.isupper() else shifted.lower())
                else:
                    result.append(char)
            return "".join(result)


        def encrypt(text):
            """Encryption is the 13-position rotation."""
            return rot13(text)


        def decrypt(text):
            """Decryption is the same 13-position rotation."""
            return rot13(text)
        """";

    private const string VigenereCode = """"
        """Vigenere Cipher: a repeated keyword selects a different Caesar shift per letter."""


        def letter_value(char):
            """A -> 0, B -> 1, ... Z -> 25."""
            return ord(char.upper()) - ord("A")


        def shift_letter(char, shift):
            """Move one letter by `shift` positions, preserving its case."""
            index = (letter_value(char) + shift) % 26
            shifted = chr(ord("A") + index)
            return shifted if char.isupper() else shifted.lower()


        def encrypt(plaintext, keyword):
            """Each keyword letter provides a shift; the keyword repeats over the text."""
            key_shifts = [letter_value(c) for c in keyword]
            output = []
            key_index = 0
            for char in plaintext:
                if "A" <= char.upper() <= "Z":
                    shift = key_shifts[key_index % len(key_shifts)]
                    output.append(shift_letter(char, shift))
                    key_index += 1
                else:
                    output.append(char)
            return "".join(output)


        def decrypt(ciphertext, keyword):
            """The same keyword is applied with negative shifts."""
            key_shifts = [letter_value(c) for c in keyword]
            output = []
            key_index = 0
            for char in ciphertext:
                if "A" <= char.upper() <= "Z":
                    shift = -key_shifts[key_index % len(key_shifts)]
                    output.append(shift_letter(char, shift))
                    key_index += 1
                else:
                    output.append(char)
            return "".join(output)
        """";

    private const string AffineCode = """"
        """Affine Cipher: E(x) = (a*x + b) mod 26, requires gcd(a, 26) = 1."""


        def gcd(a, b):
            """Greatest common divisor computed with Euclid's algorithm."""
            while b:
                a, b = b, a % b
            return a


        def mod_inverse(a, modulus):
            """Multiplicative inverse of a mod m (extended Euclid algorithm)."""
            old_r, r = a, modulus
            old_s, s = 1, 0
            while r:
                quotient = old_r // r
                old_r, r = r, old_r - quotient * r
                old_s, s = s, old_s - quotient * s
            return old_s % modulus


        def transform_char(char, a, b, inverse=False):
            """Apply E(x) or D(y) to a single letter, preserving its case."""
            if "A" <= char.upper() <= "Z":
                x = ord(char.upper()) - ord("A")
                if inverse:
                    x = mod_inverse(a, 26) * (x - b) % 26
                else:
                    x = (a * x + b) % 26
                shifted = chr(ord("A") + x)
                return shifted if char.isupper() else shifted.lower()
            return char


        def encrypt(text, a, b):
            """E(x) = (a*x + b) mod 26; `a` must be coprime with 26."""
            assert gcd(a, 26) == 1, "gcd(a, 26) must be 1"
            return "".join(transform_char(c, a, b) for c in text)


        def decrypt(text, a, b):
            """D(y) = a^-1 * (y - b) mod 26."""
            assert gcd(a, 26) == 1, "gcd(a, 26) must be 1"
            return "".join(transform_char(c, a, b, inverse=True) for c in text)
        """";

    private const string RailFenceCode = """"
        """Rail Fence Cipher: transposition that writes text in a zigzag across rails."""


        def zigzag_pattern(length, rails):
            """Return the rail index of every character position (0-based)."""
            pattern = []
            rail, direction = 0, 1
            for _ in range(length):
                pattern.append(rail)
                if rail == 0:
                    direction = 1
                elif rail == rails - 1:
                    direction = -1
                rail += direction
            return pattern


        def encrypt(text, rails):
            """Write the text in zigzag order, then read it rail by rail."""
            rows = ["" for _ in range(rails)]
            for char, rail in zip(text, zigzag_pattern(len(text), rails)):
                rows[rail] += char
            return "".join(rows)


        def decrypt(cipher, rails):
            """Rebuild the zigzag positions and refill them with ciphertext."""
            pattern = zigzag_pattern(len(cipher), rails)
            lengths = [pattern.count(rail) for rail in range(rails)]
            rows, offset = [], 0
            for size in lengths:
                rows.append(cipher[offset:offset + size])
                offset += size
            result = []
            row_indexes = [0] * rails
            for rail in pattern:
                result.append(rows[rail][row_indexes[rail]])
                row_indexes[rail] += 1
            return "".join(result)
        """";

    private const string ColumnarCode = """"
        """Columnar Transposition: fill a grid, then read columns in keyword order."""


        def column_order(key):
            """Indexes of the columns sorted by the alphabetical order of the keyword."""
            indexed = list(enumerate(key.upper()))
            indexed.sort(key=lambda pair: pair[1])
            return [index for index, _ in indexed]


        def encrypt(text, key):
            """Fill a grid row by row, then read columns in keyword order."""
            columns = len(key)
            rows = -(-len(text) // columns)          # ceiling division
            padded = text.ljust(rows * columns)      # pad the last row with spaces
            result = []
            for column in column_order(key):
                for row in range(rows):
                    result.append(padded[row * columns + column])
            return "".join(result).rstrip()


        def decrypt(cipher, key):
            """Put ciphertext back into the columns, then read row by row."""
            columns = len(key)
            rows = -(-len(cipher) // columns)
            total = rows * columns
            padded = cipher.ljust(total)
            grid = [[""] * columns for _ in range(rows)]
            index = 0
            for column in column_order(key):
                for row in range(rows):
                    grid[row][column] = padded[index]
                    index += 1
            result = []
            for row in range(rows):
                for column in range(columns):
                    result.append(grid[row][column])
            return "".join(result).rstrip()
        """";

    private const string AesCode = """"
        """AES-128 implemented from scratch: S-box, key schedule, rounds and CBC mode.

        The S-box is generated mathematically (GF(2^8) inverse + affine transform)
        instead of being copied from a table.
        """


        def gf_mult(a, b):
            """Multiply two bytes in GF(2^8) using the AES polynomial x^8+x^4+x^3+x+1."""
            result = 0
            while b:
                if b & 1:
                    result ^= a
                a <<= 1
                if a & 0x100:
                    a ^= 0x11B
                b >>= 1
            return result


        def gf_pow(base, exponent):
            """Exponentiation in GF(2^8) by square-and-multiply."""
            result = 1
            while exponent:
                if exponent & 1:
                    result = gf_mult(result, base)
                base = gf_mult(base, base)
                exponent >>= 1
            return result


        def rotl8(value, bits):
            """Rotate an 8-bit value left by `bits` positions."""
            return ((value << bits) | (value >> (8 - bits))) & 0xFF


        def build_tables():
            """Generate the S-box (GF inverse + affine transform) and its inverse."""
            sbox = [0] * 256
            for x in range(256):
                inverse = 0 if x == 0 else gf_pow(x, 254)  # x^(2^8-2) = x^-1
                value = inverse
                value ^= rotl8(inverse, 1)
                value ^= rotl8(inverse, 2)
                value ^= rotl8(inverse, 3)
                value ^= rotl8(inverse, 4)
                sbox[x] = value ^ 0x63
            inv_sbox = [0] * 256
            for i, value in enumerate(sbox):
                inv_sbox[value] = i
            return sbox, inv_sbox


        S_BOX, INV_S_BOX = build_tables()


        def key_expansion(key):
            """Expand a 16-byte key into 11 round keys of 16 bytes each."""
            rcon = 1
            words = [list(key[4 * i:4 * i + 4]) for i in range(4)]
            for i in range(4, 44):
                temp = words[i - 1][:]
                if i % 4 == 0:
                    temp = temp[1:] + temp[:1]        # RotWord
                    temp = [S_BOX[b] for b in temp]   # SubWord
                    temp[0] ^= rcon                   # Rcon
                    rcon = gf_mult(rcon, 2)
                word = [words[i - 4][j] ^ temp[j] for j in range(4)]
                words.append(word)
            round_keys = []
            for i in range(11):
                block = b""
                for word in words[4 * i:4 * i + 4]:
                    block += bytes(word)
                round_keys.append(block)
            return round_keys


        def state_from_block(block):
            """Convert 16 bytes into the 4x4 state matrix (column-major order)."""
            return [[block[4 * c + r] for c in range(4)] for r in range(4)]


        def block_from_state(state):
            """Convert the 4x4 state matrix back into 16 bytes."""
            return bytes(state[r][c] for c in range(4) for r in range(4))


        def add_round_key(state, round_key):
            """XOR the state with one 16-byte round key."""
            for row in range(4):
                for column in range(4):
                    state[row][column] ^= round_key[4 * column + row]


        def sub_bytes(state):
            """Replace every byte with its S-box value."""
            for row in range(4):
                for column in range(4):
                    state[row][column] = S_BOX[state[row][column]]


        def inv_sub_bytes(state):
            """Replace every byte with its inverse S-box value."""
            for row in range(4):
                for column in range(4):
                    state[row][column] = INV_S_BOX[state[row][column]]


        def shift_rows(state):
            """Rotate row i left by i positions."""
            for row in range(4):
                state[row] = state[row][row:] + state[row][:row]


        def inv_shift_rows(state):
            """Rotate row i right by i positions."""
            for row in range(4):
                state[row] = state[row][-row:] + state[row][:-row]


        def mix_columns(state):
            """Multiply every column by the fixed AES matrix in GF(2^8)."""
            for column in range(4):
                a = [state[row][column] for row in range(4)]
                state[0][column] = gf_mult(a[0], 2) ^ gf_mult(a[1], 3) ^ a[2] ^ a[3]
                state[1][column] = a[0] ^ gf_mult(a[1], 2) ^ gf_mult(a[2], 3) ^ a[3]
                state[2][column] = a[0] ^ a[1] ^ gf_mult(a[2], 2) ^ gf_mult(a[3], 3)
                state[3][column] = gf_mult(a[0], 3) ^ a[1] ^ a[2] ^ gf_mult(a[3], 2)


        def inv_mix_columns(state):
            """Multiply every column by the inverse AES matrix in GF(2^8)."""
            for column in range(4):
                a = [state[row][column] for row in range(4)]
                state[0][column] = gf_mult(a[0], 14) ^ gf_mult(a[1], 11) ^ gf_mult(a[2], 13) ^ gf_mult(a[3], 9)
                state[1][column] = gf_mult(a[0], 9) ^ gf_mult(a[1], 14) ^ gf_mult(a[2], 11) ^ gf_mult(a[3], 13)
                state[2][column] = gf_mult(a[0], 13) ^ gf_mult(a[1], 9) ^ gf_mult(a[2], 14) ^ gf_mult(a[3], 11)
                state[3][column] = gf_mult(a[0], 11) ^ gf_mult(a[1], 13) ^ gf_mult(a[2], 9) ^ gf_mult(a[3], 14)


        def encrypt_block(block, round_keys):
            """Encrypt one 16-byte block through the 10 AES rounds."""
            state = state_from_block(block)
            add_round_key(state, round_keys[0])
            for round_index in range(1, 10):
                sub_bytes(state)
                shift_rows(state)
                mix_columns(state)
                add_round_key(state, round_keys[round_index])
            sub_bytes(state)
            shift_rows(state)
            add_round_key(state, round_keys[10])
            return block_from_state(state)


        def decrypt_block(block, round_keys):
            """Decrypt one 16-byte block with the inverse operations."""
            state = state_from_block(block)
            add_round_key(state, round_keys[10])
            inv_shift_rows(state)
            inv_sub_bytes(state)
            for round_index in range(9, 0, -1):
                add_round_key(state, round_keys[round_index])
                inv_mix_columns(state)
                inv_shift_rows(state)
                inv_sub_bytes(state)
            add_round_key(state, round_keys[0])
            return block_from_state(state)


        def pkcs7_pad(data):
            """Pad to a multiple of 16 bytes: the padding byte repeats its own value."""
            padding = 16 - len(data) % 16
            return data + bytes([padding] * padding)


        def pkcs7_unpad(data):
            """Remove the PKCS7 padding using the last byte as the padding length."""
            padding = data[-1]
            return data[:-padding]


        def encrypt_cbc(plaintext, key, iv):
            """Pad, then XOR every block with the previous ciphertext (CBC mode)."""
            round_keys = key_expansion(key)
            padded = pkcs7_pad(plaintext)
            result = []
            previous = iv
            for offset in range(0, len(padded), 16):
                block = padded[offset:offset + 16]
                xored = bytes(a ^ b for a, b in zip(block, previous))
                encrypted = encrypt_block(xored, round_keys)
                result.append(encrypted)
                previous = encrypted
            return b"".join(result)


        def decrypt_cbc(ciphertext, key, iv):
            """Reverse the chain: decrypt, then XOR with the previous ciphertext."""
            round_keys = key_expansion(key)
            result = []
            previous = iv
            for offset in range(0, len(ciphertext), 16):
                block = ciphertext[offset:offset + 16]
                decrypted = decrypt_block(block, round_keys)
                xored = bytes(a ^ b for a, b in zip(decrypted, previous))
                result.append(xored)
                previous = block
            return pkcs7_unpad(b"".join(result))
        """";

    private const string DesCode = """"
        """DES implemented from scratch: permutations, S-boxes, Feistel network, CBC.

        The tables (IP, FP, E, P, PC-1, PC-2, S-boxes) are the official FIPS 46-3 values.
        """

        IP = [58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4,
              62, 54, 46, 38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8,
              57, 49, 41, 33, 25, 17, 9, 1, 59, 51, 43, 35, 27, 19, 11, 3,
              61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7]

        FP = [40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31,
              38, 6, 46, 14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29,
              36, 4, 44, 12, 52, 20, 60, 28, 35, 3, 43, 11, 51, 19, 59, 27,
              34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9, 49, 17, 57, 25]

        E = [32, 1, 2, 3, 4, 5, 4, 5, 6, 7, 8, 9, 8, 9, 10, 11, 12, 13,
             12, 13, 14, 15, 16, 17, 16, 17, 18, 19, 20, 21, 20, 21, 22, 23,
             24, 25, 24, 25, 26, 27, 28, 29, 28, 29, 30, 31, 32, 1]

        P = [16, 7, 20, 21, 29, 12, 28, 17, 1, 15, 23, 26, 5, 18, 31, 10,
             2, 8, 24, 14, 32, 27, 3, 9, 19, 13, 30, 6, 22, 11, 4, 25]

        PC1 = [57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34, 26, 18, 10, 2,
               59, 51, 43, 35, 27, 19, 11, 3, 60, 52, 44, 36, 63, 55, 47, 39,
               31, 23, 15, 7, 62, 54, 46, 38, 30, 22, 14, 6, 61, 53, 45, 37,
               29, 21, 13, 5, 28, 20, 12, 4]

        PC2 = [14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10, 23, 19, 12, 4,
               26, 8, 16, 7, 27, 20, 13, 2, 41, 52, 31, 37, 47, 55, 30, 40,
               51, 45, 33, 48, 44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32]

        SHIFTS = [1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1]

        S_BOXES = [
            [[14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7],
             [0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8],
             [4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0],
             [15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13]],
            [[15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10],
             [3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5],
             [0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15],
             [13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9]],
            [[10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8],
             [13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1],
             [13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7],
             [1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12]],
            [[7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15],
             [13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9],
             [10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4],
             [3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14]],
            [[2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9],
             [14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6],
             [4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14],
             [11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3]],
            [[12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11],
             [10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8],
             [9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6],
             [4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13]],
            [[4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1],
             [13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6],
             [1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2],
             [6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12]],
            [[13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7],
             [1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2],
             [7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8],
             [2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11]],
        ]


        def permute(bits, table):
            """Reorder `bits` (list of 0/1) using `table` (1-based bit positions)."""
            return [bits[pos - 1] for pos in table]


        def bytes_to_bits(data):
            """Convert bytes into a flat list of 0/1 bits."""
            return [int(bit) for byte in data for bit in format(byte, "08b")]


        def bits_to_bytes(bits):
            """Convert a flat list of 0/1 bits back into bytes."""
            result = bytearray()
            for offset in range(0, len(bits), 8):
                byte = 0
                for bit in bits[offset:offset + 8]:
                    byte = (byte << 1) | bit
                result.append(byte)
            return bytes(result)


        def generate_subkeys(key):
            """Derive the 16 subkeys of 48 bits from the 64-bit key."""
            key_bits = permute(bytes_to_bits(key), PC1)
            left, right = key_bits[:28], key_bits[28:]
            result = []
            for shift in SHIFTS:
                left = left[shift:] + left[:shift]
                right = right[shift:] + right[:shift]
                result.append(permute(left + right, PC2))
            return result


        def feistel_function(right_half, subkey):
            """F(R, K): expand to 48 bits, XOR the key, substitute, then permute."""
            expanded = permute(right_half, E)
            xored = [a ^ b for a, b in zip(expanded, subkey)]
            substituted = []
            for box_index in range(8):
                chunk = xored[box_index * 6:(box_index + 1) * 6]
                row = (chunk[0] << 1) | chunk[5]
                column = (chunk[1] << 3) | (chunk[2] << 2) | (chunk[3] << 1) | chunk[4]
                value = S_BOXES[box_index][row][column]
                substituted.extend([(value >> 3) & 1, (value >> 2) & 1,
                                    (value >> 1) & 1, value & 1])
            return permute(substituted, P)


        def des_encrypt_block(block, subkey_list):
            """Encrypt one 64-bit block through the 16 Feistel rounds."""
            bits = permute(bytes_to_bits(block), IP)
            left, right = bits[:32], bits[32:]
            for subkey in subkey_list:
                new_right = [a ^ b for a, b in zip(left, feistel_function(right, subkey))]
                left, right = right, new_right
            combined = right + left                    # swap before the final permutation
            return bits_to_bytes(permute(combined, FP))


        def des_decrypt_block(block, subkey_list):
            """Decrypt one 64-bit block using the subkeys in reverse order."""
            bits = permute(bytes_to_bits(block), IP)
            left, right = bits[:32], bits[32:]
            for subkey in reversed(subkey_list):
                new_right = [a ^ b for a, b in zip(left, feistel_function(right, subkey))]
                left, right = right, new_right
            combined = right + left
            return bits_to_bytes(permute(combined, FP))


        def pkcs7_pad(data):
            """Pad to a multiple of 8 bytes (DES block size)."""
            padding = 8 - len(data) % 8
            return data + bytes([padding] * padding)


        def pkcs7_unpad(data):
            """Remove the PKCS7 padding using the last byte as the padding length."""
            padding = data[-1]
            return data[:-padding]


        def encrypt_cbc(plaintext, key, iv):
            """Pad, then XOR every 8-byte block with the previous ciphertext (CBC)."""
            subkey_list = generate_subkeys(key)
            padded = pkcs7_pad(plaintext)
            result = []
            previous = iv
            for offset in range(0, len(padded), 8):
                block = padded[offset:offset + 8]
                xored = bytes(a ^ b for a, b in zip(block, previous))
                encrypted = des_encrypt_block(xored, subkey_list)
                result.append(encrypted)
                previous = encrypted
            return b"".join(result)


        def decrypt_cbc(ciphertext, key, iv):
            """Decrypt each block, then XOR with the previous ciphertext block."""
            subkey_list = generate_subkeys(key)
            result = []
            previous = iv
            for offset in range(0, len(ciphertext), 8):
                block = ciphertext[offset:offset + 8]
                decrypted = des_decrypt_block(block, subkey_list)
                xored = bytes(a ^ b for a, b in zip(decrypted, previous))
                result.append(xored)
                previous = block
            return pkcs7_unpad(b"".join(result))
        """";

    private const string TdesCode = """"
        """3DES: three DES passes with two or three keys (EEE2/EEE3 variants).

        Encrypt:  E = DES_encrypt(k3, DES_decrypt(k2, DES_encrypt(k1, P)))
        Decrypt:  P = DES_decrypt(k1, DES_encrypt(k2, DES_decrypt(k3, C)))
        """

        from des import des_encrypt_block, des_decrypt_block, generate_subkeys


        def split_keys(key):
            """Split a 16-byte key into (k1, k2, k1) or a 24-byte key into (k1, k2, k3)."""
            if len(key) == 16:
                return key[:8], key[8:], key[:8]
            return key[:8], key[8:16], key[16:]


        def encrypt_block(block, key):
            """Encrypt one 8-byte block with three DES passes."""
            k1, k2, k3 = [generate_subkeys(part) for part in split_keys(key)]
            first = des_encrypt_block(block, k1)
            second = des_decrypt_block(first, k2)
            return des_encrypt_block(second, k3)


        def decrypt_block(block, key):
            """Decrypt one 8-byte block with the three passes in reverse order."""
            k1, k2, k3 = [generate_subkeys(part) for part in split_keys(key)]
            first = des_decrypt_block(block, k3)
            second = des_encrypt_block(first, k2)
            return des_decrypt_block(second, k1)


        def pkcs7_pad(data):
            """Pad to a multiple of 8 bytes (DES block size)."""
            padding = 8 - len(data) % 8
            return data + bytes([padding] * padding)


        def pkcs7_unpad(data):
            """Remove the PKCS7 padding using the last byte as the padding length."""
            padding = data[-1]
            return data[:-padding]


        def encrypt_cbc(plaintext, key, iv):
            """Pad, then XOR every 8-byte block with the previous ciphertext (CBC)."""
            padded = pkcs7_pad(plaintext)
            result = []
            previous = iv
            for offset in range(0, len(padded), 8):
                block = padded[offset:offset + 8]
                xored = bytes(a ^ b for a, b in zip(block, previous))
                encrypted = encrypt_block(xored, key)
                result.append(encrypted)
                previous = encrypted
            return b"".join(result)


        def decrypt_cbc(ciphertext, key, iv):
            """Decrypt each block, then XOR with the previous ciphertext block."""
            result = []
            previous = iv
            for offset in range(0, len(ciphertext), 8):
                block = ciphertext[offset:offset + 8]
                decrypted = decrypt_block(block, key)
                xored = bytes(a ^ b for a, b in zip(decrypted, previous))
                result.append(xored)
                previous = block
            return pkcs7_unpad(b"".join(result))
        """";

    private const string RsaCode = """"
        """RSA from scratch: modular exponentiation, key generation, encrypt/decrypt.

        The core functions (primality, inverse, exponentiation) are implemented
        directly; only random number generation uses the standard library.
        """

        import random


        def gcd(a, b):
            """Greatest common divisor (Euclid's algorithm)."""
            while b:
                a, b = b, a % b
            return a


        def extended_gcd(a, b):
            """Return (g, x, y) with g = gcd(a, b) and a*x + b*y = g."""
            if b == 0:
                return a, 1, 0
            g, x1, y1 = extended_gcd(b, a % b)
            return g, y1, x1 - (a // b) * y1


        def mod_inverse(a, modulus):
            """Multiplicative inverse of a mod m (works for any modulus)."""
            g, x, _ = extended_gcd(a, modulus)
            assert g == 1, "a is not invertible modulo m"
            return x % modulus


        def mod_exp(base, exponent, modulus):
            """Fast exponentiation: base^exponent mod modulus (square-and-multiply)."""
            result = 1
            base %= modulus
            while exponent:
                if exponent & 1:
                    result = result * base % modulus
                base = base * base % modulus
                exponent >>= 1
            return result


        def is_prime(n, rounds=40):
            """Miller-Rabin primality test; rounds control the confidence level."""
            if n < 2:
                return False
            for small in (2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37):
                if n % small == 0:
                    return n == small
            d, s = n - 1, 0
            while d % 2 == 0:
                d //= 2
                s += 1
            for _ in range(rounds):
                a = random.randrange(2, n - 1)
                x = mod_exp(a, d, n)
                if x in (1, n - 1):
                    continue
                for _ in range(s - 1):
                    x = x * x % n
                    if x == n - 1:
                        break
                else:
                    return False
            return True


        def generate_prime(bits):
            """Generate a probable prime with the given bit length."""
            while True:
                candidate = random.getrandbits(bits)
                candidate |= (1 << bits - 1) | 1     # set top bit and odd
                if is_prime(candidate):
                    return candidate


        def generate_keypair(bits=1024):
            """Generate an RSA key pair: public (e, n) and private (d, n)."""
            p = generate_prime(bits // 2)
            q = generate_prime(bits // 2)
            n = p * q
            phi = (p - 1) * (q - 1)
            e = 65537
            while gcd(e, phi) != 1:
                e = random.randrange(3, phi, 2)
            d = mod_inverse(e, phi)
            return (e, n), (d, n)


        def encrypt(plaintext, public_key):
            """Encrypt bytes with the public key: c = m^e mod n (block by block).

            Every block is prefixed with its byte length so that decryption can
            restore the exact message even when the last block is shorter.
            """
            e, n = public_key
            chunk = (n.bit_length() - 1) // 8 - 1      # payload bytes per block
            cipher_blocks = []
            for offset in range(0, len(plaintext), chunk):
                data = plaintext[offset:offset + chunk]
                block = bytes([len(data)]) + data
                m = int.from_bytes(block, "big")
                cipher_blocks.append(mod_exp(m, e, n))
            return cipher_blocks


        def decrypt(cipher_blocks, private_key):
            """Decrypt blocks with the private key: m = c^d mod n."""
            d, n = private_key
            plaintext = bytearray()
            for c in cipher_blocks:
                m = mod_exp(c, d, n)
                size = (m.bit_length() + 7) // 8
                block = m.to_bytes(size, "big")
                length = block[0]
                plaintext.extend(block[1:1 + length])
            return bytes(plaintext)
        """";

    private const string Md5Code = """"
        """MD5 from scratch: Merkle-Damgard construction, 64 rounds, 128-bit digest.

        The 64 round constants are generated from sin(i) exactly as the standard says.
        """

        import math


        def left_rotate(value, amount):
            """Rotate a 32-bit word left by `amount` positions."""
            return ((value << amount) | (value >> (32 - amount))) & 0xFFFFFFFF


        def pad_message(data):
            """Append the 0x80 byte, zeros, and the 64-bit length (low word first)."""
            message = bytearray(data)
            length_bits = len(message) * 8
            message.append(0x80)
            while len(message) % 64 != 56:
                message.append(0)
            message.extend(length_bits.to_bytes(8, "little"))
            return bytes(message)


        def md5(data):
            """Compute the 128-bit MD5 digest of `data` (bytes)."""
            a0, b0, c0, d0 = (0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476)
            shifts = [7, 12, 17, 22] * 4 + [5, 9, 14, 20] * 4 + [4, 11, 16, 23] * 4 + [6, 10, 15, 21] * 4
            constants = [int(2 ** 32 * abs(math.sin(i + 1))) & 0xFFFFFFFF for i in range(64)]

            message = pad_message(data)
            for offset in range(0, len(message), 64):
                block = message[offset:offset + 64]
                words = [int.from_bytes(block[i:i + 4], "little") for i in range(0, 64, 4)]
                a, b, c, d = a0, b0, c0, d0
                for i in range(64):
                    if i < 16:
                        f = (b & c) | (~b & d)
                        g = i
                    elif i < 32:
                        f = (d & b) | (~d & c)
                        g = (5 * i + 1) % 16
                    elif i < 48:
                        f = b ^ c ^ d
                        g = (3 * i + 5) % 16
                    else:
                        f = c ^ (b | ~d)
                        g = (7 * i) % 16
                    f = (f + a + constants[i] + words[g]) & 0xFFFFFFFF
                    a, d, c, b = d, c, b, (b + left_rotate(f, shifts[i])) & 0xFFFFFFFF
                a0 = (a0 + a) & 0xFFFFFFFF
                b0 = (b0 + b) & 0xFFFFFFFF
                c0 = (c0 + c) & 0xFFFFFFFF
                d0 = (d0 + d) & 0xFFFFFFFF

            digest = bytearray()
            for value in (a0, b0, c0, d0):
                digest.extend(value.to_bytes(4, "little"))
            return bytes(digest)


        def hexdigest(data):
            """Return the MD5 digest as a lowercase hexadecimal string."""
            return md5(data).hex()
        """";

    private const string Sha1Code = """"
        """SHA-1 from scratch: 80 rounds, 160-bit digest, Merkle-Damgard construction."""


        def left_rotate(value, amount):
            """Rotate a 32-bit word left by `amount` positions."""
            return ((value << amount) | (value >> (32 - amount))) & 0xFFFFFFFF


        def pad_message(data):
            """Append the 0x80 byte, zeros, and the 64-bit big-endian length."""
            message = bytearray(data)
            length_bits = len(message) * 8
            message.append(0x80)
            while len(message) % 64 != 56:
                message.append(0)
            message.extend(length_bits.to_bytes(8, "big"))
            return bytes(message)


        def sha1(data):
            """Compute the 160-bit SHA-1 digest of `data` (bytes)."""
            h0, h1, h2, h3, h4 = (0x67452301, 0xEFCDAB89, 0x98BADCFE,
                                   0x10325476, 0xC3D2E1F0)

            message = pad_message(data)
            for offset in range(0, len(message), 64):
                block = message[offset:offset + 64]
                words = [int.from_bytes(block[i:i + 4], "big") for i in range(0, 64, 4)]
                for i in range(16, 80):
                    mixed = words[i - 3] ^ words[i - 8] ^ words[i - 14] ^ words[i - 16]
                    words.append(left_rotate(mixed, 1))

                a, b, c, d, e = h0, h1, h2, h3, h4
                for i in range(80):
                    if i < 20:
                        f = (b & c) | (~b & d)
                        constant = 0x5A827999
                    elif i < 40:
                        f = b ^ c ^ d
                        constant = 0x6ED9EBA1
                    elif i < 60:
                        f = (b & c) | (b & d) | (c & d)
                        constant = 0x8F1BBCDC
                    else:
                        f = b ^ c ^ d
                        constant = 0xCA62C1D6
                    temp = (left_rotate(a, 5) + f + e + constant + words[i]) & 0xFFFFFFFF
                    e, d, c, b, a = d, c, left_rotate(b, 30), a, temp

                h0 = (h0 + a) & 0xFFFFFFFF
                h1 = (h1 + b) & 0xFFFFFFFF
                h2 = (h2 + c) & 0xFFFFFFFF
                h3 = (h3 + d) & 0xFFFFFFFF
                h4 = (h4 + e) & 0xFFFFFFFF

            digest = bytearray()
            for value in (h0, h1, h2, h3, h4):
                digest.extend(value.to_bytes(4, "big"))
            return bytes(digest)


        def hexdigest(data):
            """Return the SHA-1 digest as a lowercase hexadecimal string."""
            return sha1(data).hex()
        """";

    private const string Sha256Code = """"
        """SHA-256 from scratch: 64 rounds, 256-bit digest, Merkle-Damgard construction.

        The 64 round constants are derived from the fractional parts of the cube
        roots of the first 64 prime numbers, exactly as the standard specifies.
        High-precision decimal arithmetic keeps all 32 bits exact.
        """

        from decimal import Decimal, getcontext

        getcontext().prec = 80


        def right_rotate(value, amount):
            """Rotate a 32-bit word right by `amount` positions."""
            return ((value >> amount) | (value << (32 - amount))) & 0xFFFFFFFF


        def first_primes(count):
            """Return the first `count` prime numbers (sieve of Eratosthenes)."""
            primes, candidate = [], 2
            while len(primes) < count:
                if all(candidate % p for p in primes if p * p <= candidate):
                    primes.append(candidate)
                candidate += 1
            return primes


        def frac_bits(root, bits):
            """Extract the first `bits` bits of the fractional part of a Decimal."""
            return int((root - int(root)) * (Decimal(2) ** bits))


        def build_constants():
            """Initial hash values (sqrt of primes) and round constants (cbrt)."""
            sqrt_values = [frac_bits(Decimal(p).sqrt(), 32) for p in first_primes(8)]
            one_third = Decimal(1) / Decimal(3)
            cbrt_values = [frac_bits(Decimal(p) ** one_third, 32) for p in first_primes(64)]
            return sqrt_values, cbrt_values


        H, K = build_constants()


        def pad_message(data):
            """Append the 0x80 byte, zeros, and the 64-bit big-endian length."""
            message = bytearray(data)
            length_bits = len(message) * 8
            message.append(0x80)
            while len(message) % 64 != 56:
                message.append(0)
            message.extend(length_bits.to_bytes(8, "big"))
            return bytes(message)


        def sha256(data):
            """Compute the 256-bit SHA-256 digest of `data` (bytes)."""
            h0, h1, h2, h3, h4, h5, h6, h7 = H
            message = pad_message(data)

            for offset in range(0, len(message), 64):
                block = message[offset:offset + 64]
                words = [int.from_bytes(block[i:i + 4], "big") for i in range(0, 64, 4)]
                for i in range(16, 64):
                    s0 = right_rotate(words[i - 15], 7) ^ right_rotate(words[i - 15], 18) ^ (words[i - 15] >> 3)
                    s1 = right_rotate(words[i - 2], 17) ^ right_rotate(words[i - 2], 19) ^ (words[i - 2] >> 10)
                    words.append((words[i - 16] + s0 + words[i - 7] + s1) & 0xFFFFFFFF)

                a, b, c, d, e, f, g, h = h0, h1, h2, h3, h4, h5, h6, h7
                for i in range(64):
                    s1 = right_rotate(e, 6) ^ right_rotate(e, 11) ^ right_rotate(e, 25)
                    ch = (e & f) ^ (~e & g)
                    temp1 = (h + s1 + ch + K[i] + words[i]) & 0xFFFFFFFF
                    s0 = right_rotate(a, 2) ^ right_rotate(a, 13) ^ right_rotate(a, 22)
                    maj = (a & b) ^ (a & c) ^ (b & c)
                    temp2 = (s0 + maj) & 0xFFFFFFFF
                    h, g, f, e, d, c, b, a = g, f, e, (d + temp1) & 0xFFFFFFFF, c, b, a, (temp1 + temp2) & 0xFFFFFFFF

                h0 = (h0 + a) & 0xFFFFFFFF
                h1 = (h1 + b) & 0xFFFFFFFF
                h2 = (h2 + c) & 0xFFFFFFFF
                h3 = (h3 + d) & 0xFFFFFFFF
                h4 = (h4 + e) & 0xFFFFFFFF
                h5 = (h5 + f) & 0xFFFFFFFF
                h6 = (h6 + g) & 0xFFFFFFFF
                h7 = (h7 + h) & 0xFFFFFFFF

            digest = bytearray()
            for value in (h0, h1, h2, h3, h4, h5, h6, h7):
                digest.extend(value.to_bytes(4, "big"))
            return bytes(digest)


        def hexdigest(data):
            """Return the SHA-256 digest as a lowercase hexadecimal string."""
            return sha256(data).hex()
        """";

    private const string Sha512Code = """"
        """SHA-512 from scratch: 80 rounds, 512-bit digest, 64-bit words.

        The 80 round constants are derived from the fractional parts of the cube
        roots of the first 80 prime numbers, exactly as the standard specifies.
        High-precision decimal arithmetic keeps all 64 bits exact.
        """

        from decimal import Decimal, getcontext

        getcontext().prec = 80


        def right_rotate(value, amount):
            """Rotate a 64-bit word right by `amount` positions."""
            return ((value >> amount) | (value << (64 - amount))) & 0xFFFFFFFFFFFFFFFF


        def first_primes(count):
            """Return the first `count` prime numbers (sieve of Eratosthenes)."""
            primes, candidate = [], 2
            while len(primes) < count:
                if all(candidate % p for p in primes if p * p <= candidate):
                    primes.append(candidate)
                candidate += 1
            return primes


        def frac_bits(root, bits):
            """Extract the first `bits` bits of the fractional part of a Decimal."""
            return int((root - int(root)) * (Decimal(2) ** bits))


        def build_constants():
            """Initial hash values (sqrt of primes) and round constants (cbrt)."""
            sqrt_values = [frac_bits(Decimal(p).sqrt(), 64) for p in first_primes(8)]
            one_third = Decimal(1) / Decimal(3)
            cbrt_values = [frac_bits(Decimal(p) ** one_third, 64) for p in first_primes(80)]
            return sqrt_values, cbrt_values


        H, K = build_constants()


        def pad_message(data):
            """Append the 0x80 byte, zeros, and the 128-bit big-endian length."""
            message = bytearray(data)
            length_bits = len(message) * 8
            message.append(0x80)
            while len(message) % 128 != 112:
                message.append(0)
            message.extend(length_bits.to_bytes(16, "big"))
            return bytes(message)


        def sha512(data):
            """Compute the 512-bit SHA-512 digest of `data` (bytes)."""
            h0, h1, h2, h3, h4, h5, h6, h7 = H
            message = pad_message(data)

            for offset in range(0, len(message), 128):
                block = message[offset:offset + 128]
                words = [int.from_bytes(block[i:i + 8], "big") for i in range(0, 128, 8)]
                for i in range(16, 80):
                    s0 = right_rotate(words[i - 15], 1) ^ right_rotate(words[i - 15], 8) ^ (words[i - 15] >> 7)
                    s1 = right_rotate(words[i - 2], 19) ^ right_rotate(words[i - 2], 61) ^ (words[i - 2] >> 6)
                    words.append((words[i - 16] + s0 + words[i - 7] + s1) & 0xFFFFFFFFFFFFFFFF)

                a, b, c, d, e, f, g, h = h0, h1, h2, h3, h4, h5, h6, h7
                for i in range(80):
                    s1 = right_rotate(e, 14) ^ right_rotate(e, 18) ^ right_rotate(e, 41)
                    ch = (e & f) ^ (~e & g)
                    temp1 = (h + s1 + ch + K[i] + words[i]) & 0xFFFFFFFFFFFFFFFF
                    s0 = right_rotate(a, 28) ^ right_rotate(a, 34) ^ right_rotate(a, 39)
                    maj = (a & b) ^ (a & c) ^ (b & c)
                    temp2 = (s0 + maj) & 0xFFFFFFFFFFFFFFFF
                    h, g, f, e, d, c, b, a = g, f, e, (d + temp1) & 0xFFFFFFFFFFFFFFFF, c, b, a, (temp1 + temp2) & 0xFFFFFFFFFFFFFFFF

                h0 = (h0 + a) & 0xFFFFFFFFFFFFFFFF
                h1 = (h1 + b) & 0xFFFFFFFFFFFFFFFF
                h2 = (h2 + c) & 0xFFFFFFFFFFFFFFFF
                h3 = (h3 + d) & 0xFFFFFFFFFFFFFFFF
                h4 = (h4 + e) & 0xFFFFFFFFFFFFFFFF
                h5 = (h5 + f) & 0xFFFFFFFFFFFFFFFF
                h6 = (h6 + g) & 0xFFFFFFFFFFFFFFFF
                h7 = (h7 + h) & 0xFFFFFFFFFFFFFFFF

            digest = bytearray()
            for value in (h0, h1, h2, h3, h4, h5, h6, h7):
                digest.extend(value.to_bytes(8, "big"))
            return bytes(digest)


        def hexdigest(data):
            """Return the SHA-512 digest as a lowercase hexadecimal string."""
            return sha512(data).hex()
        """";
}