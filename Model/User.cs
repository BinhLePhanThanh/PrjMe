public class User
{
    public long Id { get; set; }

    // 🧑‍🎓 Học sinh
    public string? StudentCode { get; set; } 
    public string? FullName { get; set; } 
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? ClassName { get; set; }

    // 👨‍👩‍👧 Phụ huynh
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }

    // 📱 SĐT (chỉ 1 số)
    public string? Phone { get; set; } 

    // 📲 Zalo IDs (update thường xuyên)
    public string? StudentZaloUserId { get; set; }
    public string? ParentZaloUserId { get; set; }

    public DateTime? StudentLastSeenAt { get; set; }
    public DateTime? ParentLastSeenAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}