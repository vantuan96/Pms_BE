using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.Business.Helper
{
    public class HelperBusiness
    {
        private IUnitOfWork unitOfWork = new EfUnitOfWork();
        private static HelperBusiness _instant { get; set; }
        public static HelperBusiness Instant
        {
            get
            {
                if (_instant == null)
                {
                    _instant = new HelperBusiness();
                }
                if(_instant.unitOfWork==null)
                    _instant.unitOfWork= new EfUnitOfWork();
                return _instant;
            }
            set
            {
                _instant = value;
            }
        }
        public void SetAppContant()
        {
            var value1= this.ListGroupCodeIsIncludeChildPackage;
            var value2 = this.ListGroupCodeIsMaternityPackage;
            var value3 = this.ListGroupCodeIsVaccinePackage;
            var value4 = this.ListGroupCodeIsMCRPackage;
            //linhht bundle payment
            var value5 = this.ListGroupCodeIsBundlePackage;
        }
        private List<string> _ListGroupCodeIsIncludeChildPackage;
        public List<string> ListGroupCodeIsIncludeChildPackage
        {
            get {
                if (this._ListGroupCodeIsIncludeChildPackage == null)
                {
                    var entity = unitOfWork.AppConstantRepository.FirstOrDefault(x => x.ConstantKey == Constant.Key_ListGroupCodeIsIncludeChildPackage && x.IsActived);
                    if (entity != null && !string.IsNullOrEmpty(entity.ConstantValue))
                    {
                        this._ListGroupCodeIsIncludeChildPackage= entity.ConstantValue.Split(',').ToList();
                        return _ListGroupCodeIsIncludeChildPackage;
                    }
                    return Constant.ListGroupCodeIsIncludeChildPackage;
                }
                return this._ListGroupCodeIsIncludeChildPackage;
            }
        }
        private List<string> _ListGroupCodeIsMaternityPackage;
        public List<string> ListGroupCodeIsMaternityPackage
        {
            get
            {
                if (this._ListGroupCodeIsMaternityPackage == null)
                {
                    var entity = unitOfWork.AppConstantRepository.FirstOrDefault(x => x.ConstantKey == Constant.Key_ListGroupCodeIsMaternityPackage && x.IsActived);
                    if (entity != null && !string.IsNullOrEmpty(entity.ConstantValue))
                    {
                        this._ListGroupCodeIsMaternityPackage = entity.ConstantValue.Split(',').ToList();
                        return _ListGroupCodeIsMaternityPackage;
                    }
                    return Constant.ListGroupCodeIsMaternityPackage;
                }
                return this._ListGroupCodeIsMaternityPackage;
            }
        }
        /// <summary>
        /// linhht config gói Bundle Payment
        /// </summary>
        private List<string> _ListGroupCodeIsBundlePackage;
        public List<string> ListGroupCodeIsBundlePackage
        {
            get
            {
                if (this._ListGroupCodeIsBundlePackage == null)
                {
                    var entity = unitOfWork.AppConstantRepository.FirstOrDefault(x => x.ConstantKey == Constant.Key_ListGroupCodeIsBundlePackage && x.IsActived);
                    if (entity != null && !string.IsNullOrEmpty(entity.ConstantValue))
                    {
                        this._ListGroupCodeIsBundlePackage = entity.ConstantValue.Split(',').ToList();
                        return _ListGroupCodeIsBundlePackage;
                    }
                    return Constant.ListGroupCodeIsBundlePackage;
                }
                return this._ListGroupCodeIsBundlePackage;
            }
        }
        private List<string> _ListGroupCodeIsVaccinePackage;
        public List<string> ListGroupCodeIsVaccinePackage
        {
            get
            {
                if (this._ListGroupCodeIsVaccinePackage == null)
                {
                    var entity = unitOfWork.AppConstantRepository.FirstOrDefault(x => x.ConstantKey == Constant.Key_ListGroupCodeIsVaccinePackage && x.IsActived);
                    if (entity != null && !string.IsNullOrEmpty(entity.ConstantValue))
                    {
                        this._ListGroupCodeIsVaccinePackage = entity.ConstantValue.Split(',').ToList();
                        return _ListGroupCodeIsVaccinePackage;
                    }
                    return Constant.ListGroupCodeIsVaccinePackage;
                }
                return this._ListGroupCodeIsVaccinePackage;
            }
        }
        private List<string> _ListGroupCodeIsMCRPackage;
        public List<string> ListGroupCodeIsMCRPackage
        {
            get
            {
                if (this._ListGroupCodeIsMCRPackage == null)
                {
                    var entity = unitOfWork.AppConstantRepository.FirstOrDefault(x => x.ConstantKey == Constant.Key_ListGroupCodeIsMCRPackage && x.IsActived);
                    if (entity != null && !string.IsNullOrEmpty(entity.ConstantValue))
                    {
                        this._ListGroupCodeIsMCRPackage = entity.ConstantValue.Split(',').ToList();
                        return _ListGroupCodeIsMCRPackage;
                    }
                    return Constant.ListGroupCodeIsMCRPackage;
                }
                return this._ListGroupCodeIsMCRPackage;
            }
        }
    }
}
