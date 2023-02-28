using System;
using System.Collections;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    public class ThreadInfo
    {
        #region Config Calculate thread
        private int _calculateRevenueThreads = 1;
        [XmlElement("CALCULATE_REVENUE_THREADS")]
        public int CALCULATE_REVENUE_THREADS
        {
            get { return _calculateRevenueThreads; }
            set { _calculateRevenueThreads = value; }
        }
        private int _calculateRevenueSpeed = 1000;
        [XmlElement("CALCULATE_REVENUE_SPEED")]
        public int CALCULATE_REVENUE_SPEED
        {
            get { return _calculateRevenueSpeed; }
            set { _calculateRevenueSpeed = value; }
        }
        private int _calculateRevenueAttemps = 5;
        [XmlElement("CALCULATE_REVENUE_ATTEMPS")]
        public int CALCULATE_REVENUE_ATTEMPS
        {
            get { return _calculateRevenueAttemps; }
            set { _calculateRevenueAttemps = value; }
        }
        #endregion

        #region Config HisRevenue Update thread
        private int _hisRevenueUpdateThreads = 1;
        [XmlElement("HIS_REVENUE_UPDATE_THREADS")]
        public int HIS_REVENUE_UPDATE_THREADS
        {
            get { return _hisRevenueUpdateThreads; }
            set { _hisRevenueUpdateThreads = value; }
        }
        private int _hisRevenueUpdateSpeed = 1000;
        [XmlElement("HIS_REVENUE_UPDATE_SPEED")]
        public int HIS_REVENUE_UPDATE_SPEED
        {
            get { return _hisRevenueUpdateSpeed; }
            set { _hisRevenueUpdateSpeed = value; }
        }
        private int _hisRevenueUpdateAttemps = 5;
        [XmlElement("HIS_REVENUE_UPDATE_ATTEMPS")]
        public int HIS_REVENUE_UPDATE_ATTEMPS
        {
            get { return _hisRevenueUpdateAttemps; }
            set { _hisRevenueUpdateAttemps = value; }
        }
        #endregion

        #region Config Get His Revenue thread
        private int _getRevenueThreads = 1;
        [XmlElement("GET_REVENUE_THREADS")]
        public int GET_REVENUE_THREADS
        {
            get { return _getRevenueThreads; }
            set { _getRevenueThreads = value; }
        }
        private int _getRevenueSpeed = 1000;
        [XmlElement("GET_REVENUE_SPEED")]
        public int GET_REVENUE_SPEED
        {
            get { return _getRevenueSpeed; }
            set { _getRevenueSpeed = value; }
        }
        private int _getRevenueItemsGet = 100;
        [XmlElement("GET_REVENUE_ITEMSGET")]
        public int GET_REVENUE_ITEMSGET
        {
            get { return _getRevenueItemsGet; }
            set { _getRevenueItemsGet = value; }
        }
        private int _getRevenueHowItem2Get = 10;
        [XmlElement("GET_REVENUE_HOWITEMS2GET")]
        public int GET_REVENUE_HOWITEMS2GET
        {
            get { return _getRevenueHowItem2Get; }
            set { _getRevenueHowItem2Get = value; }
        }
        #endregion

        #region Config Sync User|AD thread
        private int _syncUserThreads = 1;
        [XmlElement("USER_SYNC_THREADS")]
        public int USER_SYNC_THREADS
        {
            get { return _syncUserThreads; }
            set { _syncUserThreads = value; }
        }
        private int _syncUserSpeed = 1000;
        [XmlElement("USER_SYNC_SPEED")]
        public int USER_SYNC_SPEED
        {
            get { return _syncUserSpeed; }
            set { _syncUserSpeed = value; }
        }
        private int _syncUserAttemps = 5;
        [XmlElement("USER_SYNC_ATTEMPS")]
        public int USER_SYNC_ATTEMPS
        {
            get { return _syncUserAttemps; }
            set { _syncUserAttemps = value; }
        }
        #endregion

        #region Config Get HisCharge for update Dims thread
        private int _getHisCharge4UpdateDimsThreads = 1;
        [XmlElement("GETHISCHARGE4UPDATEDIMS_THREADS")]
        public int GETHISCHARGE4UPDATEDIMS_THREADS
        {
            get { return _getHisCharge4UpdateDimsThreads; }
            set { _getHisCharge4UpdateDimsThreads = value; }
        }
        private int _getHisCharge4UpdateDimsSpeed = 1000;
        [XmlElement("GETHISCHARGE4UPDATEDIMS_SPEED")]
        public int GETHISCHARGE4UPDATEDIMS_SPEED
        {
            get { return _getHisCharge4UpdateDimsSpeed; }
            set { _getHisCharge4UpdateDimsSpeed = value; }
        }
        private int _getHisCharge4UpdateDimsAttemps = 5;
        [XmlElement("GETHISCHARGE4UPDATEDIMS_ATTEMPS")]
        public int GETHISCHARGE4UPDATEDIMS_ATTEMPS
        {
            get { return _getHisCharge4UpdateDimsAttemps; }
            set { _getHisCharge4UpdateDimsAttemps = value; }
        }
        #endregion

        #region Config Get HisCharge for update Dims thread
        private int _updateDimsRevenueThreads = 1;
        [XmlElement("UPDATEDIMSREVENUE_THREADS")]
        public int UPDATEDIMSREVENUE_THREADS
        {
            get { return _updateDimsRevenueThreads; }
            set { _updateDimsRevenueThreads = value; }
        }
        private int _updateDimsRevenueSpeed = 1000;
        [XmlElement("UPDATEDIMSREVENUE_SPEED")]
        public int UPDATEDIMSREVENUE_SPEED
        {
            get { return _updateDimsRevenueSpeed; }
            set { _updateDimsRevenueSpeed = value; }
        }
        private int _updateDimsRevenueAttemps = 5;
        [XmlElement("UPDATEDIMSREVENUE_ATTEMPS")]
        public int UPDATEDIMSREVENUE_ATTEMPS
        {
            get { return _updateDimsRevenueAttemps; }
            set { _updateDimsRevenueAttemps = value; }
        }
        #endregion

        #region Config Get PatientInPackage for update Using stat thread
        private int _getPatientInPackage4UpdateUsingThreads = 1;
        [XmlElement("GETPATIENTINPACKAGE4UPDATEUSING_THREADS")]
        public int GETPATIENTINPACKAGE4UPDATEUSING_THREADS
        {
            get { return _getPatientInPackage4UpdateUsingThreads; }
            set { _getPatientInPackage4UpdateUsingThreads = value; }
        }
        private int _getPatientInPackage4UpdateUsingSpeed = 1000;
        [XmlElement("GETPATIENTINPACKAGE4UPDATEUSING_SPEED")]
        public int GETPATIENTINPACKAGE4UPDATEUSING_SPEED
        {
            get { return _getPatientInPackage4UpdateUsingSpeed; }
            set { _getPatientInPackage4UpdateUsingSpeed = value; }
        }
        private int _getPatientInPackage4UpdateUsingAttemps = 5;
        [XmlElement("GETPATIENTINPACKAGE4UPDATEUSING_ATTEMPS")]
        public int GETPATIENTINPACKAGE4UPDATEUSING_ATTEMPS
        {
            get { return _getHisCharge4UpdateDimsAttemps; }
            set { _getHisCharge4UpdateDimsAttemps = value; }
        }
        #endregion

        #region Config update patient in package using stat thread
        private int _updatePatientInPackageUsingThreads = 1;
        [XmlElement("UPDATEPATIENTINPACKAGEUSING_THREADS")]
        public int UPDATEPATIENTINPACKAGEUSING_THREADS
        {
            get { return _updatePatientInPackageUsingThreads; }
            set { _updatePatientInPackageUsingThreads = value; }
        }
        private int _updatePatientInPackageUsingSpeed = 1000;
        [XmlElement("UPDATEPATIENTINPACKAGEUSING_SPEED")]
        public int UPDATEPATIENTINPACKAGEUSING_SPEED
        {
            get { return _updatePatientInPackageUsingSpeed; }
            set { _updatePatientInPackageUsingSpeed = value; }
        }
        private int _updatePatientInPackageUsingAttemps = 5;
        [XmlElement("UPDATEPATIENTINPACKAGEUSING_ATTEMPS")]
        public int UPDATEPATIENTINPACKAGEUSING_ATTEMPS
        {
            get { return _updatePatientInPackageUsingAttemps; }
            set { _updatePatientInPackageUsingAttemps = value; }
        }
        #endregion
    }
}
