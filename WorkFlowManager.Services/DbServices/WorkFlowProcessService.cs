using AutoMapper;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using WorkFlowManager.Common.DataAccess._UnitOfWork;
using WorkFlowManager.Common.Dto;
using WorkFlowManager.Common.Enums;
using WorkFlowManager.Common.Tables;
using WorkFlowManager.Common.Validation;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Helper;

namespace WorkFlowManager.Services.DbServices
{
    public abstract class WorkFlowProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly WorkFlowDataService _workFlowDataService;

        public abstract void OzelFormKaydet(WorkFlowFormViewModel formData);
        public abstract bool FullFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState);

        public WorkFlowProcessService(IUnitOfWork unitOfWork, WorkFlowDataService workFlowDataService)
        {
            _unitOfWork = unitOfWork;
            _workFlowDataService = workFlowDataService;
        }

        protected int GetWorkFlowOwnerId(string id)
        {
            WorkFlowTrace workFlowTrace = _unitOfWork.Repository<WorkFlowTrace>().Get(int.Parse(id));
            return workFlowTrace.OwnerId;
        }


        public virtual void AddOrUpdate(WorkFlowTrace workFlowTrace)
        {
            WorkFlowTrace workFlowTraceDB = null;
            if (workFlowTrace.Id == 0)
            {
                workFlowTraceDB = Mapper.Map<WorkFlowTrace, WorkFlowTrace>(workFlowTrace);
                _unitOfWork.Repository<WorkFlowTrace>().Add(workFlowTraceDB);
            }
            else
            {
                workFlowTraceDB = _unitOfWork.Repository<WorkFlowTrace>().Get(workFlowTrace.Id);
                Mapper.Map(workFlowTrace, workFlowTraceDB);
                _unitOfWork.Repository<WorkFlowTrace>().Update(workFlowTraceDB);
            }
            _unitOfWork.Complete();
            workFlowTrace.Id = workFlowTraceDB.Id;
        }

        public string KararNoktasiSurecKontrolJobCallBase(string id, string jobId, string hourInterval)
        {
            WorkFlowTrace torSatinAlmaWorkFlowTrace = _unitOfWork.Repository<WorkFlowTrace>().Get(int.Parse(id));

            torSatinAlmaWorkFlowTrace.JobId = jobId;
            _unitOfWork.Repository<WorkFlowTrace>().Update(torSatinAlmaWorkFlowTrace);
            _unitOfWork.Complete();
            return "OK";
        }

        public string GetIsAkisi(string gorevAkis, int WorkFlowTraceId)
        {
            var workFlowTraceListesi =
                _workFlowDataService
                    .GetIslemListesi();

            UserProcessViewModel kullaniciIslem =
                workFlowTraceListesi
                    .Single(x => x.Id == WorkFlowTraceId);

            IEnumerable<UserProcessViewModel> workFlowTraceListesiForOwner =
                workFlowTraceListesi
                    .Where(x => x.OwnerId == kullaniciIslem.OwnerId);

            Dictionary<string, int> workFlowTraceTekrarSayisi = new Dictionary<string, int>();
            Dictionary<string, string> workFlowTraceDurumClassi = new Dictionary<string, string>();
            foreach (var workFlowTrace in workFlowTraceListesiForOwner)
            {
                int tekrarSayisi;
                string durumClassi = "";
                string dummy = "";
                if (workFlowTraceTekrarSayisi.TryGetValue(workFlowTrace.ProcessUniqueCode, out tekrarSayisi))
                {
                    tekrarSayisi++;
                    workFlowTraceTekrarSayisi[workFlowTrace.ProcessUniqueCode] = tekrarSayisi;
                }
                else
                {
                    tekrarSayisi++;
                    workFlowTraceTekrarSayisi.Add(workFlowTrace.ProcessUniqueCode, tekrarSayisi);
                }


                if (workFlowTrace.ProcessStatus == Common.Enums.ProcessStatus.Draft)
                {
                    durumClassi = "current";
                }
                else if (workFlowTrace.ProcessStatus == Common.Enums.ProcessStatus.Cancelled)
                {
                    durumClassi = "cancelled";
                }
                else //Tamamlanmışsa ikinci kez tekrar edilip edilmediği kontrol edilecek
                {
                    if (tekrarSayisi > 1)
                    {
                        durumClassi = "revized";
                    }
                    else
                    {
                        durumClassi = "completed";
                    }
                }
                if (workFlowTraceDurumClassi.TryGetValue(workFlowTrace.ProcessUniqueCode, out dummy))
                {
                    workFlowTraceDurumClassi[workFlowTrace.ProcessUniqueCode] = durumClassi;
                }
                else
                {
                    workFlowTraceDurumClassi.Add(workFlowTrace.ProcessUniqueCode, durumClassi);
                }
            }

            StringBuilder statusString = new StringBuilder(
            @"classDef completed fill:#9f6,stroke:#333,stroke-width:2px;" +
            "classDef current fill: yellow,stroke:#333,stroke-width:2px;" +
            "classDef cancelled fill: #e55c59,stroke:#333,stroke-width:2px;" +
            "classDef revized fill:#196,stroke:#333,stroke-width:2px;");
            statusString.Append("\\n");
            foreach (var WorkFlowTraceDurum in workFlowTraceDurumClassi)
            {
                statusString.Append(string.Format("class {0} {1};\\n", WorkFlowTraceDurum.Key, WorkFlowTraceDurum.Value));
            }
            return string.Format("{0}\\n{1}", gorevAkis, statusString.ToString());
        }

        public IEnumerable<UserProcessViewModel> WorkFlowProcessList(int ownerId)
        {
            return
                _workFlowDataService
                    .GetIslemListesi()
                        .Where(x => x.OwnerId == ownerId);
        }





        public List<WorkFlowTraceVM> GorevListesiOlustur(int workFlowTraceId)
        {
            UserProcessViewModel kullaniciWorkFlowTrace =
            _workFlowDataService
                .GetIslemListesi()
                    .Single(x => x.Id == workFlowTraceId);

            ProcessVM sonrakiGorev = null;


            var processList = _workFlowDataService.GetGorevIslemListesi(kullaniciWorkFlowTrace.TaskId);

            if (kullaniciWorkFlowTrace.NextProcessId != null)
            {
                sonrakiGorev =
                    processList
                        .Where(x => x.Id == (int)kullaniciWorkFlowTrace.NextProcessId)
                            .FirstOrDefault();
            }


            var workFlowTraceListesi = WorkFlowProcessList(kullaniciWorkFlowTrace.OwnerId);

            List<WorkFlowTraceVM> tumWorkFlowTraceler = workFlowTraceListesi
                .OrderBy(x => x.LastlyModifiedTime)
                .Select(x => new WorkFlowTraceVM
                {
                    Description = x.ProcessDescription,
                    ProcessName = x.ProcessName,
                    ProcessId = x.ProcessId,
                    ProcessStatus = x.ProcessStatus,
                    ConditionOptionId = x.ConditionOptionId,
                    OwnerId = x.OwnerId,
                    AssignedRole = x.AssignedRole,
                    Id = x.Id
                })
                .ToList();
            List<WorkFlowTraceVM> progressGosterilecekWorkFlowTraceler = new List<WorkFlowTraceVM>();

            if (tumWorkFlowTraceler.Count() > 1)
            {
                for (int i = 0; i < tumWorkFlowTraceler.Count(); i++)
                {

                    if ((i + 1) < tumWorkFlowTraceler.Count() && tumWorkFlowTraceler[i + 1].Id == workFlowTraceId)
                    {
                        progressGosterilecekWorkFlowTraceler.Add(tumWorkFlowTraceler[i]);
                        progressGosterilecekWorkFlowTraceler.Add(tumWorkFlowTraceler[i + 1]);
                        break;
                    }
                }
            }
            else
            {
                progressGosterilecekWorkFlowTraceler.Add(tumWorkFlowTraceler[0]);
            }


            if (sonrakiGorev != null)
            {
                progressGosterilecekWorkFlowTraceler.Add(new WorkFlowTraceVM
                {
                    ProcessStatus = null,
                    AssignedRole = sonrakiGorev.AssignedRole,
                    Description = sonrakiGorev.Description,
                    ProcessName = sonrakiGorev.Name
                });
            }

            return progressGosterilecekWorkFlowTraceler;

        }

        public List<UserProcessViewModel> GeriGidilebilecekWorkFlowTraceListesi(int WorkFlowTraceId)
        {
            var birimdekiTumWorkFlowTraceler = _workFlowDataService.GetIslemListesi();

            UserProcessViewModel kullaniciWorkFlowTrace =
                    birimdekiTumWorkFlowTraceler
                        .Where(x => x.Id == WorkFlowTraceId)
                            .FirstOrDefault();

            var tumIslemler =
                birimdekiTumWorkFlowTraceler
                    .Where(x => x.OwnerId == kullaniciWorkFlowTrace.OwnerId && x.ProcessStatus == Common.Enums.ProcessStatus.Completed && x.Id < WorkFlowTraceId)
                        .OrderByDescending(x => x.Id);

            List<UserProcessViewModel> oncekiWorkFlowTraceListesi = new List<UserProcessViewModel>();
            var taskId = kullaniciWorkFlowTrace.TaskId;//WorkFlowTrace.GorevWorkFlowTrace.GorevId;
            var WorkFlowTraceiYapanRol = kullaniciWorkFlowTrace.AssignedRole;
            var gorevWorkFlowTraceId = kullaniciWorkFlowTrace.ProcessId;
            var gorevWorkFlowTraceListesi = _workFlowDataService.GetGorevIslemListesi(taskId);

            foreach (var oncekiIslem in tumIslemler)
            {
                ProcessVM gorevWorkFlowTrace = gorevWorkFlowTraceListesi.SingleOrDefault(x => x.Id == oncekiIslem.ProcessId);

                if (gorevWorkFlowTrace.AssignedRole == ProjectRole.Sistem)
                {
                    continue;
                }
                if (gorevWorkFlowTrace.AssignedRole != WorkFlowTraceiYapanRol)
                {
                    break;
                }

                if (gorevWorkFlowTrace.IsCondition)
                {
                    gorevWorkFlowTrace = gorevWorkFlowTraceListesi.SingleOrDefault(x => x.Id == oncekiIslem.ConditionOptionId);
                }

                //Güncel işlem tekrar çağrılmamalı
                //Listeye eklenen birdaha eklenmemeli
                if (oncekiIslem.ProcessId != gorevWorkFlowTraceId && !oncekiWorkFlowTraceListesi.Any(x => oncekiIslem.ProcessId == x.ProcessId))
                {
                    //Eklenecek işlem akış içerisinde geri alınmak istenen işlemden sonra olmamalı
                    List<int> elementOfTree = new List<int>();
                    if (!SonrakiWorkFlowTraceMi(elementOfTree, gorevWorkFlowTraceListesi, gorevWorkFlowTraceId, oncekiIslem.ProcessId))
                    {
                        oncekiWorkFlowTraceListesi.Add(oncekiIslem);
                    }
                }
            }
            return oncekiWorkFlowTraceListesi.OrderBy(x => x.Id).ToList();
        }

        public bool SonrakiWorkFlowTraceMi(List<int> elementOfTree, IEnumerable<ProcessVM> gorevWorkFlowTraceListesi, int gorevWorkFlowTraceId, int kontrolEdilecekGorevWorkFlowTraceId)
        {
            if (elementOfTree.Any(x => x == gorevWorkFlowTraceId))
            {
                return false;
            }

            var gorevWorkFlowTrace = gorevWorkFlowTraceListesi.SingleOrDefault(x => x.Id == gorevWorkFlowTraceId);
            var sonuc = false;
            elementOfTree.Add(gorevWorkFlowTraceId);

            if (gorevWorkFlowTrace.IsCondition)
            {
                //Self referansları almayacak şekilde seçenekler listeye alınıyor
                var secenekListesi = gorevWorkFlowTraceListesi.Where(x => x.ConditionId == gorevWorkFlowTrace.Id && x.NextProcessId != gorevWorkFlowTrace.Id);
                foreach (var secenek in secenekListesi)
                {
                    if (SonrakiWorkFlowTraceMi(elementOfTree, gorevWorkFlowTraceListesi, secenek.Id, kontrolEdilecekGorevWorkFlowTraceId))
                    {
                        sonuc = true;
                        break;
                    }
                }
            }
            else
            {
                if (gorevWorkFlowTrace.NextProcessId != null)
                {
                    if (gorevWorkFlowTrace.NextProcessId == kontrolEdilecekGorevWorkFlowTraceId)
                    {
                        sonuc = true;
                    }
                    else
                    {
                        sonuc = SonrakiWorkFlowTraceMi(elementOfTree, gorevWorkFlowTraceListesi, (int)gorevWorkFlowTrace.NextProcessId, kontrolEdilecekGorevWorkFlowTraceId);
                    }
                }
            }
            return sonuc;
        }

        public virtual WorkFlowTrace WorkFlowTraceDetail(int workFlowTraceId)
        {
            WorkFlowTrace WorkFlowTrace = _unitOfWork.Repository<WorkFlowTrace>()
                .Get(
                        x => x.Id == workFlowTraceId,
                        x => x.Process,
                            x => x.Process.DocumentList,
                                x => x.Process.FormView,
                                    x => x.DocumentList,
                                        x => x.Process.Task
                    );

            if (WorkFlowTrace.Process.GetType() == typeof(Condition))
            {
                ((Condition)WorkFlowTrace.Process).OptionList = _unitOfWork.Repository<ConditionOption>().GetList(x => x.ConditionId == WorkFlowTrace.ProcessId).ToList();
            }

            return WorkFlowTrace;
        }

        public virtual void WorkFlowFormKaydet<TClass, TVM>(WorkFlowFormViewModel workFlowFormViewModel)
        where TClass : class
        where TVM : WorkFlowFormViewModel
        {
            TClass form = _unitOfWork.Repository<TClass>().Get(workFlowFormViewModel.WorkFlowIslemForm.OwnerId);


            if (form != null)
            {
                Mapper.Map((TVM)workFlowFormViewModel, form);
                _unitOfWork.Repository<TClass>().Update(form);
            }
            else
            {
                form = (TClass)Activator.CreateInstance(typeof(TClass));
                Mapper.Map((TVM)workFlowFormViewModel, form);
                _unitOfWork.Repository<TClass>().Add(form);
            }
            _unitOfWork.Complete();
        }


        public WorkFlowFormViewModel WorkFlowFormYukle<T>(int torId, WorkFlowTraceForm workFlowTraceForm, List<FormObject> tamamlanmisFormlar) where T : WorkFlowFormViewModel
        {
            WorkFlowFormViewModel workFlowForm = null;

            if (typeof(T) != typeof(WorkFlowFormViewModel))
            {
                ServiceDTO serviceDTO = new ServiceDTO(workFlowTraceForm);

                var formObject = tamamlanmisFormlar.FirstOrDefault(x => x.ConstantObject.ViewModelType() == typeof(T));

                if (formObject != null)
                {
                    formObject.ConstantObject.BelgeTuruAyarla(serviceDTO);
                    workFlowForm = formObject.ConstantObject.Yukle(serviceDTO);
                }
            }

            if (workFlowForm == null)
            {
                workFlowForm = new WorkFlowFormViewModel();
            }

            workFlowForm.WorkFlowIslemForm = workFlowTraceForm;
            return workFlowForm;

        }


        public void OzelFormKaydet(WorkFlowFormViewModel formData, List<FormObject> tamamlanmisFormlar)
        {
            var formObject = tamamlanmisFormlar.FirstOrDefault(x => x.ConstantObject.ViewModelType() == formData.GetType());

            if (formObject != null)
            {
                formObject.ConstantObject.Kaydet(formData);
            }
        }


        public bool OzelFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState, List<FormObject> tamamlanmisFormlar)
        {

            var formObject = tamamlanmisFormlar.FirstOrDefault(x => x.ConstantObject.ViewModelType() == formData.GetType());

            if (formObject != null)
            {
                return formObject.ConstantObject.Validate(formData, modelState);
            }

            return true;
        }


        public UserProcessViewModel GetKullaniciProcessVM(int workFlowTraceId)
        {
            return _workFlowDataService.GetIslemListesi().SingleOrDefault(x => x.Id == workFlowTraceId);
        }

        public virtual void WorkFlowProcessIptal(int workFlowTraceId)
        {
            WorkFlowTrace workFlowTrace = WorkFlowTraceDetail(workFlowTraceId);

            if (workFlowTrace.Process.GetType() == typeof(DecisionPoint))
            {
                DecisionPoint kararNoktasi = (DecisionPoint)workFlowTrace.Process;

                if (kararNoktasi.CancelProcessId != null)
                {
                    int cancelProcessId = (int)kararNoktasi.CancelProcessId;

                    if (workFlowTrace.JobId != null)
                    {
                        RecurringJob.RemoveIfExists(workFlowTrace.JobId);
                    }
                    _unitOfWork.Repository<WorkFlowTrace>().Remove(workFlowTraceId);
                    YeniWorkFlowTraceOlustur(cancelProcessId, workFlowTrace.OwnerId);
                }
            }
        }

        /// <summary>
        /// Akış içerisinde hedef olarak belirlenen önce ki bir adıma işlemi geri alır.
        /// </summary>
        /// <param name="WorkFlowTraceId">Geri alınacak işlem Id si</param>
        public virtual void WorkFlowWorkFlowTraceiGeriAl(int WorkFlowTraceId, int hedefGorevWorkFlowTraceId)
        {

            WorkFlowTrace WorkFlowTrace = _unitOfWork.Repository<WorkFlowTrace>().Get(WorkFlowTraceId);

            WorkFlowTrace.ProcessStatus = ProcessStatus.Cancelled;

            AddOrUpdate(WorkFlowTrace);

            YeniWorkFlowTraceOlustur(hedefGorevWorkFlowTraceId, WorkFlowTrace.OwnerId);

        }



        public virtual void WorkFlowWorkFlowNextProcess(int ownerId)
        {
            var WorkFlowTraceListesi = WorkFlowProcessList(ownerId);

            UserProcessViewModel torSatinAlmaWorkFlowTraceSuAnKi = WorkFlowTraceListesi.OrderByDescending(x => x.Id).First();//Geri doğru sıraladığımız için ilkini aldık
            int suAnKiWorkFlowTraceId = torSatinAlmaWorkFlowTraceSuAnKi.ProcessId;
            if (torSatinAlmaWorkFlowTraceSuAnKi.ConditionOptionId != null) //Koşula ait seçenek seçilmiş ise
            {
                suAnKiWorkFlowTraceId = (int)torSatinAlmaWorkFlowTraceSuAnKi.ConditionOptionId;
            }

            WorkFlowTrace torSatinAlmaWorkFlowTraceSuAnKiDB = _unitOfWork.Repository<WorkFlowTrace>().Get(torSatinAlmaWorkFlowTraceSuAnKi.Id);
            torSatinAlmaWorkFlowTraceSuAnKiDB.ProcessStatus = Common.Enums.ProcessStatus.Completed;

            AddOrUpdate(torSatinAlmaWorkFlowTraceSuAnKiDB);

            Process suAnKiGorevWorkFlowTrace = _unitOfWork.Repository<Process>().Get(x => x.Id == suAnKiWorkFlowTraceId, x => x.NextProcess);

            if (suAnKiGorevWorkFlowTrace.NextProcessId != null)//Sonra ki kaydı varsa devam edilecek
            {
                YeniWorkFlowTraceOlustur(suAnKiGorevWorkFlowTrace.NextProcess, ownerId);
            }
        }

        private void YeniWorkFlowTraceOlustur(int sonrakiGorevWorkFlowTraceId, int ownerId)
        {
            var sonrakiWorkFlowTrace = _unitOfWork.Repository<Process>().Get(sonrakiGorevWorkFlowTraceId);
            YeniWorkFlowTraceOlustur(sonrakiWorkFlowTrace, ownerId);
        }


        private void YeniWorkFlowTraceOlustur(Process sonrakiWorkFlowTrace, int ownerId)
        {
            WorkFlowTrace torSatinAlmaWorkFlowTrace = new WorkFlowTrace()
            {
                ProcessId = sonrakiWorkFlowTrace.Id,
                OwnerId = ownerId,
                ProcessStatus = ProcessStatus.Draft
            };
            AddOrUpdate(torSatinAlmaWorkFlowTrace);

            //Sonraki işlem sistem tarafından yapılan karar noktası ise ilgili metod çalıştırılıp sonuca göre devam edilecek
            if (sonrakiWorkFlowTrace is DecisionPoint)
            {
                KararNoktasiSurecKontrol(torSatinAlmaWorkFlowTrace.Id);
            }
        }




        /// <summary>
        /// Karar noktası olarak oluşturulmuş <see cref="GorevWorkFlowTrace"/> e ait <see cref="KararNoktasi.KararMetot"/> larının ilgili parametrelerle çalıştırılarak(<see cref="DynamicMethodCallService.Caller(IUnitOfWork, MerkezBankasiService, UserService, string, CacheService)"/>) 
        /// sürecin <see cref="GorevWorkFlowTrace.SonrakiId"/> adımına geçirilmesinde kullanılır. <param name="WorkFlowTraceId"></param> parametresinden <see cref="WorkFlowTrace"/> objesi çağrılarak <see cref="WorkFlowTrace.GorevWorkFlowTraceId"/> değerinden karar noktasına ulaşılır.
        /// </summary>
        /// <remarks>
        /// İşlem bilgilerini hızlı sorgulamak için cache yapısı <see cref="CacheService.GetWorkFlowTraceListesi(<param name="birimId"/>)"/> şeklinde birim tabanlı kullanılmaktadır.
        /// Yeni işlem ekleme sonrasında işlemlerin tutulduğu cache <see cref="CacheService.ClearWorkFlowTraceListesi(<param name="birimId"/>)"/> şeklinde silinmektedir.
        /// Her <see cref="KararNoktasi"/> objesinin <see cref="KosulSecenek"/> türünde bağlı kayıtları bulunmaktadır.  <see cref="KararNoktasi.KararMetot"/> çalışması sonrasında referans edilen <see cref="KosulSecenek.SonrakiId"/>
        /// <see cref="KararNoktasi.Id"/> ile eşit olması durumunda sistemin düzenli olarak karar metodunu çalıştırması için <see cref="SatinAlmaService.KararNoktasiSurecKontrolJobCall(string, string, string, string)"/> çalışması gerekmetedir.
        /// </remarks>
        public void KararNoktasiSurecKontrol(int WorkFlowTraceId)
        {
            WorkFlowTrace workFlowTraceDecisionPoint = _unitOfWork.Repository<WorkFlowTrace>().Get(WorkFlowTraceId);

            DecisionPoint kararNoktasi = _unitOfWork.Repository<DecisionPoint>().Get(x => x.Id == workFlowTraceDecisionPoint.ProcessId, x => x.DecisionMethod, x => x.Task);

            if (!string.IsNullOrEmpty(kararNoktasi.DecisionMethod.MethodFunction))
            {
                string serviceName = kararNoktasi.Task.MethodServiceName;
                string methodName = string.Format("{0}.{1}", serviceName, kararNoktasi.DecisionMethod.MethodFunction.Substring(0, kararNoktasi.DecisionMethod.MethodFunction.LastIndexOf("(")));

                var parameters = kararNoktasi.DecisionMethod.MethodFunction.Substring(
                                        kararNoktasi.DecisionMethod.MethodFunction.LastIndexOf("(") + 1,
                                            (kararNoktasi.DecisionMethod.MethodFunction.LastIndexOf(")") - kararNoktasi.DecisionMethod.MethodFunction.LastIndexOf("(") - 1))
                                                .Split(',')
                                                    .Select(p => p.Trim())
                                                        .ToList();


                List<object> parametersValue = new List<object>();

                foreach (var parameter in parameters)
                {
                    if (!string.IsNullOrWhiteSpace(parameter))
                    {
                        var value = workFlowTraceDecisionPoint.GetType().GetProperty(parameter).GetValue(workFlowTraceDecisionPoint, null);
                        parametersValue.Add(value);
                    }
                }

                var parametersArray = parametersValue.ToArray();

                var methodCallString = string.Format("{0}({1})", methodName, string.Join(",", parametersArray));

                var kararMethodSonuc =
                    DynamicMethodCallService.Caller(
                        _unitOfWork,
                                    methodCallString,
                                        _workFlowDataService);

                Process kosulSecenek = _unitOfWork.Repository<ConditionOption>().Get(x => x.ConditionId == kararNoktasi.Id && x.Value == kararMethodSonuc);
                if (kosulSecenek.NextProcessId == kararNoktasi.Id)
                {

                    if (workFlowTraceDecisionPoint.JobId == null)//Job oluşturulmamışsa
                    {
                        string jobId = Guid.NewGuid().ToString();
                        parametersValue.Add(jobId);
                        parametersValue.Add(kararNoktasi.RepetitionFrequenceByHour);
                        parametersArray = parametersValue.ToArray();

                        var methodJobCallString = string.Format("{0}.KararNoktasiSurecKontrolJobCall({1})", serviceName, string.Join(",", parametersArray));

                        DynamicMethodCallService.Caller(
                            _unitOfWork,
                                    methodJobCallString,
                                        _workFlowDataService);

                    }
                }
                else
                {
                    workFlowTraceDecisionPoint.ConditionOptionId = kosulSecenek.Id;
                    AddOrUpdate(workFlowTraceDecisionPoint);
                    if (workFlowTraceDecisionPoint.JobId != null)
                    {
                        RecurringJob.RemoveIfExists(workFlowTraceDecisionPoint.JobId);
                    }
                    WorkFlowWorkFlowNextProcess(workFlowTraceDecisionPoint.OwnerId);
                }
            }
        }

        public bool StandartFormKontrol(WorkFlowTraceForm formData, ModelStateDictionary modelState)
        {
            return ValidationHelper.Validate(formData, new WorkFlowTraceFormValidator(_unitOfWork), modelState);
        }

        public bool FullFormValidate(WorkFlowFormViewModel formData, ModelStateDictionary modelState, List<FormObject> tamamlanmisFormlar)
        {
            bool resultStandartForm = StandartFormKontrol(formData.WorkFlowIslemForm, modelState);
            bool resultOzelForm = OzelFormValidate(formData, modelState, tamamlanmisFormlar);

            return resultOzelForm && resultStandartForm;
        }

        /// <summary>
        /// Parametre olarak girilen <see cref="GorevWorkFlowTrace"/> i sonra ki adıma ilerletir ya da iptal sonrasi gitmesi gereken adıma götürür.
        /// </summary>
        /// <remarks>
        /// İşlem iptali ya da ilerletilmesi sonucunda cache sıfırlanarak yeni sorguların sağlıklı yapılması sağlanır.
        /// Bir sonra ki işlemi aynı rol yapacaksa kullanıcıya bir sonra ki işlemi otomatik olarak açılır. Ya da ana menüye dönmesi sağlanır.
        /// </remarks>
        /// <param name="WorkFlowTraceId">O an ki işlemin Id sini ifade eder</param>
        /// <param name="ownerId">İşlemin bağlı olduğu ana kaydı ifade eder <see cref="Tor"/> ya da <see cref="Lot"/> Id si olabilir</param>
        /// <param name="birimId">İşlemin bağlı olduğu ana kaydın birimini ifade eder</param>
        /// <param name="WorkFlowTraceIptalId">İptal edilecekse gideceği işlemin Id sini tanımlar</param>
        /// <param name="WorkFlowTraceIptal">İşlemin iptal edilip edilmeyeceğini belirtir</param>
        /// <returns>Gidilecek route, mesaj ve mesaj tipi dönülür</returns>
        public object SonrakiSurecKontrolu(int WorkFlowTraceId)
        {
            WorkFlowTrace WorkFlowTrace = WorkFlowTraceDetail(WorkFlowTraceId);
            int gorevWorkFlowTraceId = WorkFlowTrace.ProcessId;

            Process gorevWorkFlowTrace = _unitOfWork.Repository<Process>().Get(gorevWorkFlowTraceId);

            var WorkFlowTraceListesi = WorkFlowProcessList(WorkFlowTrace.OwnerId);
            UserProcessViewModel sonWorkFlowTrace = WorkFlowTraceListesi.OrderByDescending(x => x.Id).First();
            int sonWorkFlowTraceId = sonWorkFlowTrace.ProcessId;

            bool sonrakiWorkFlowTraceiKendisiYapacak = false;
            if (gorevWorkFlowTrace.Id != sonWorkFlowTraceId)
            {
                sonrakiWorkFlowTraceiKendisiYapacak = true; //_userService.WorkFlowTraceSirasiGelmis(sonWorkFlowTrace);
            }

            object routeObject = null;
            if (sonrakiWorkFlowTraceiKendisiYapacak)
            {
                routeObject = new { workFlowTraceId = sonWorkFlowTrace.Id };
            }
            else
            {
                routeObject = new { controller = "Home" };
            }
            return routeObject;
        }

        public bool WorkFlowYetkiKontrol(UserProcessViewModel kullaniciWorkFlowTraceVM)
        {
            bool rslt = true;
            if (
                !(
                        (
                            kullaniciWorkFlowTraceVM.AssignedRole != ProjectRole.Sistem
                            &&
                            true //_userService.ProjeRoluVar(kullaniciWorkFlowTraceVM.ProjeId, kullaniciWorkFlowTraceVM.WorkFlowTraceiYapanRol)
                        )
                        ||
                        (
                            kullaniciWorkFlowTraceVM.AssignedRole == ProjectRole.Sistem
                            &&
                            true //_userService.ProjeErisimYetkisiVar(kullaniciWorkFlowTraceVM.ProjeId)
                        )
                )
               )

            {
                var errorMessage = "Sayfaya Erişim Yetkiniz Yok!";
                NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(errorMessage);

                rslt = false;
                //return RedirectToAction("Index", new { controller = "Home" }).WithMessage(this, errorMessage, MessageType.Danger);
            }

            return rslt;
        }

        public WorkFlowDTO WorkFlowBaseInfo(UserProcessViewModel kullaniciWorkFlowTraceVM)
        {
            return
                new WorkFlowDTO
                {
                    //Controller = kullaniciWorkFlowTraceVM.Controller,
                    //SpecialFormTemplateView = kullaniciWorkFlowTraceVM.SpecialFormTemplateView,
                    //TraceId = kullaniciWorkFlowTraceVM.Id,
                    //TaskName = kullaniciWorkFlowTraceVM.TaskName,
                    //ProcessId = kullaniciWorkFlowTraceVM.ProcessId,
                    //TaskId = kullaniciWorkFlowTraceVM.TaskId,
                    //IsCondition = kullaniciWorkFlowTraceVM.IsCondition
                    GeriGidilebilecekIslemListesi = kullaniciWorkFlowTraceVM.ProcessStatus != ProcessStatus.Completed ? GeriGidilebilecekWorkFlowTraceListesi(kullaniciWorkFlowTraceVM.Id) : new List<UserProcessViewModel>(),
                    GormeyeYetkiliIslemListesi = WorkFlowProcessList(kullaniciWorkFlowTraceVM.OwnerId).Where(x => x.ProcessStatus != ProcessStatus.Draft),
                    ProgressGorevListesi = GorevListesiOlustur(kullaniciWorkFlowTraceVM.Id),
                };
        }

        public void SetSatinAlmaWorkFlowTraceForm(WorkFlowTraceForm satinAlmaWorkFlowTraceForm, WorkFlowDTO workFlowBase)
        {
            satinAlmaWorkFlowTraceForm.ProgressGorevListesi = workFlowBase.ProgressGorevListesi;
            satinAlmaWorkFlowTraceForm.GeriGidilebilecekIslemListesi = workFlowBase.GeriGidilebilecekIslemListesi;
            satinAlmaWorkFlowTraceForm.GormeyeYetkiliIslemListesi = workFlowBase.GormeyeYetkiliIslemListesi;
            //satinAlmaWorkFlowTraceForm.ProcessTaskId = workFlowBase.TaskId;
            //satinAlmaWorkFlowTraceForm.ProcessTaskName = workFlowBase.TaskName;
            //satinAlmaWorkFlowTraceForm.IsCondition = workFlowBase.IsCondition;

            if (satinAlmaWorkFlowTraceForm.IsCondition)
            {
                satinAlmaWorkFlowTraceForm.ListOfOptions =
                    _workFlowDataService
                        .GetGorevIslemListesi(satinAlmaWorkFlowTraceForm.ProcessTaskId)
                            .Where(x => x.ConditionId == satinAlmaWorkFlowTraceForm.ProcessId)
                                .ToList();
            }
        }


        public UserProcessViewModel UserLastProcessViewModel(int ownerId)
        {
            IEnumerable<UserProcessViewModel> WorkFlowTraceListesi =
                _workFlowDataService
                    .GetIslemListesi()
                        .Where(x => x.OwnerId == ownerId);

            var sonWorkFlowTrace = WorkFlowTraceListesi.OrderBy(x => x.Id).LastOrDefault();
            if (sonWorkFlowTrace == null)
            {
                sonWorkFlowTrace = new UserProcessViewModel
                {
                    Id = 0
                };
            }
            return sonWorkFlowTrace;
        }
    }
}
