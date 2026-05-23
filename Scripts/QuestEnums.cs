// Dosya Adı: QuestEnums.cs
public enum QuestState
{
    REQUIREMENTS_NOT_MET, // Henüz Darion bu görevi alamaz
    CAN_START,            // NPC üzerinde ünlem çıkabilir
    IN_PROGRESS,          // Görev devam ediyor
    CAN_FINISH,           // Tüm adımlar bitti, ödül alınabilir
    FINISHED              // Tamamlandı
}

public enum QuestType
{
    MAIN, // Ana Hikaye
    SIDE, // Yan Görev
    COMPLETED // Tamamlananlar (Bunu QuestState'ten de çekebiliriz ama filtreleme için burada olması rahat olur)
}