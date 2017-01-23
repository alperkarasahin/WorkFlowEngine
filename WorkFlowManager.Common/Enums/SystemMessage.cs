using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorkFlowManager.Common.Enums
{
    public enum SystemMessage
    {
        [Description("Kayıt başarı ile gerçekleşti.")]
        [Display(Name = "Kayıt başarı ile gerçekleşti.")]
        RecordSuccess = 10,

        [Description("Kayıt sırasında hata oluştu.")]
        [Display(Name = "Kayıt sırasında hata oluştu.")]
        RecordFail = 20,

        [Description("Dosya Erişim Yetkisi Yok.")]
        [Display(Name = "Dosya Erişim Yetkisi Yok.")]
        FileAccessDenied = 30,


        [Description("ToR oluşturuldu.")]
        [Display(Name = "ToR oluşturuldu.")]
        ToRCreated = 40,

        [Description("Erişim Yetkisi Yok.")]
        [Display(Name = "Erişim Yetkisi Yok.")]
        AccessDenied = 50

    }
}
