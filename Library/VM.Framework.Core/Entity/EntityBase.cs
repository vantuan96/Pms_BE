﻿using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;
using System.Web.Security;


namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// This is the base class from which most business objects will be derived. 
    /// To create a business object, inherit from this class.
    /// </summary>
    /// <typeparam name="TYPE">The type of the derived class.</typeparam>
    /// <typeparam name="KEY">The type of the Id property.</typeparam>
    [Serializable]
    public abstract class EntityBase<TYPE, KEY> :
        IDataErrorInfo,
        INotifyPropertyChanged,
        IChangeTracking,
        IDisposable,
        IBaseEntity
        where TYPE : EntityBase<TYPE, KEY>, new()
    {
        #region Inited Properties

        private KEY _Id;
        /// <summary>
        /// Gets the unique Identification of the object.
        /// </summary>
        public KEY Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        private DateTime _DateCreated = DateTime.MinValue;
        /// <summary>
        /// The date on which the instance was created.
        /// </summary>
        public DateTime DateCreated
        {
            get
            {
                if (_DateCreated == DateTime.MinValue)
                    return _DateCreated;

                return _DateCreated;
            }
            set
            {
                if (_DateCreated != value) MarkChanged("DateCreated");
                _DateCreated = value;
            }
        }


        private DateTime? _DateModified = null;
        /// <summary>
        /// The date on which the instance was modified.
        /// </summary>
        public DateTime? DateModified
        {
            get
            {
                return _DateModified;
            }
            set
            {
                if (_DateModified != value) MarkChanged("DateModified");
                _DateModified = value;
            }
        }

        #endregion

        #region IBaseEntity Properties

        private bool _IsNew = true;
        /// <summary>
        /// Gets if this is a new object, False if it is a pre-existing object.
        /// </summary>
        public bool IsNew
        {
            get { return _IsNew; }
        }

        private bool _IsDeleted;
        /// <summary>
        /// Gets if this object is marked for deletion.
        /// </summary>
        public bool IsDeleted
        {
            get { return _IsDeleted; }
        }

        private bool _IsChanged = true;
        /// <summary>
        /// Gets if this object's data has been changed.
        /// </summary>
        public virtual bool IsChanged
        {
            get { return _IsChanged; }
        }

        /// <summary>
        /// Gets whether the object is valid or not.
        /// </summary>
        public virtual bool IsValid
        {
            get
            {
                ValidationRules();
                return this._BrokenRules.Count == 0;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated
        {
            get
            {
                return Thread.CurrentPrincipal.Identity.IsAuthenticated;
            }
        }

        public virtual bool CanAdd
        {
            get
            {
                return CheckRoleInList(AddRoleList);
            }
        }

        public virtual bool CanDelete
        {
            get
            {
                return CheckRoleInList(DeleteRoleList);
            }
        }

        public virtual bool CanEdit
        {
            get
            {
                return CheckRoleInList(EditRoleList);
            }
        }

        public virtual bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// The roles for the entity can bed added.
        /// Example Administrators,ArticleEditors
        /// </summary>
        public string AddRoleList { get; set; }

        /// <summary>
        /// The roles for the entity can bed edited.
        /// Example Administrators,ArticleEditors
        /// </summary>
        public string EditRoleList { get; set; }

        /// <summary>
        /// The roles for the entity can bed deleted.
        /// Example Administrators,ArticleEditors
        /// </summary>
        public string DeleteRoleList { get; set; }


        private bool CheckRoleInList(string roleList)
        {
            if (!string.IsNullOrEmpty(roleList))
            {
                string[] roles = roleList.Split(new char[] { ',' });

                foreach (string r in roles)
                {
                    if (Thread.CurrentPrincipal.IsInRole(r))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Deleteing and Changing

        /// <summary>
        /// Marks the object for deletion. It will then be 
        /// deleted when the object's Save() method is called.
        /// </summary>
        public void MarkDeleted()
        {
            _IsDeleted = true;
            _IsChanged = true;
        }


        private StringCollection _ChangedProperties = new StringCollection();
        /// <summary>
        /// A collection of the properties that have 
        /// been marked as being dirty.
        /// </summary>
        protected virtual StringCollection ChangedProperties
        {
            get { return _ChangedProperties; }
        }

        /// <summary>
        /// Marks an object as being dirty, or changed.
        /// </summary>
        /// <param name="propertyName">The name of the property to mark dirty.</param>
        protected virtual void MarkChanged(string propertyName)
        {
            _IsChanged = true;
            if (!_ChangedProperties.Contains(propertyName))
            {
                _ChangedProperties.Add(propertyName);
            }

            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Marks the object as being an clean, 
        /// which means not dirty.
        /// </summary>
        public virtual void MarkOld()
        {
            _IsChanged = false;
            _IsNew = false;
            _ChangedProperties.Clear();
        }

        /// <summary>
        /// Check whether or not the specified property has been changed
        /// </summary>
        /// <param name="propertyName">The name of the property to check.</param>
        protected bool IsPropertyDirty(string propertyName)
        {
            return _ChangedProperties.Contains(propertyName.ToLowerInvariant());
        }

        /// <summary>
        /// Check whether or not the specified properties has been changed
        /// </summary>
        /// <param name="propertyNames">The names of the properties to check.</param>
        /// <returns>True if all of the specified properties have been changed.</returns>
        protected bool IsPropertyDirty(string[] propertyNames)
        {
            foreach (string name in propertyNames)
            {
                if (!_ChangedProperties.Contains(name.ToUpperInvariant()))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Loading and Saving

        /// <summary>
        /// Loads an instance of the object based on the Id.
        /// </summary>
        /// <param name="id">The unique identifier of the object</param>
        public static TYPE Load(KEY id)
        {
            TYPE instance = new TYPE();
            instance = instance.DataSelect(id);
            //instance.Id = id;
            //instance = instance.DataSelectAll();

            if (instance != null)
            {
                instance.MarkOld();
                return instance;
            }

            return null;
        }

        /// <summary>
        /// Saves the object to the data store (inserts, updates or deletes).
        /// </summary>
        virtual public SaveAction Save()
        {
            return Save(string.Empty, string.Empty);
        }

        /// <summary>
        /// Saves the object to the data store (inserts, updates or deletes).
        /// </summary>
        virtual public SaveAction Save(string userName, string password)
        {
            bool isValidated;

            if (userName == string.Empty)
                isValidated = false;
            else
                isValidated = Membership.ValidateUser(userName, password);

            if (IsDeleted && !(IsAuthenticated || isValidated))
                throw new System.Security.SecurityException("You are not authorized to delete the object");

            if (!IsValid && !IsDeleted)
                throw new InvalidOperationException(ValidationMessage);

            if (IsDisposed && !IsDeleted)
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "You cannot save a disposed {0}", this.GetType().Name));

            if (IsChanged)
            {
                return Update();
            }

            return SaveAction.None;
        }

        /// <summary>
        /// Is called by the save method when the object is old and dirty.
        /// </summary>
        private SaveAction Update()
        {
            SaveAction action = SaveAction.None;
            bool saveSucceeded = false;

            if (this.IsDeleted)
            {
                if (!this.IsNew)
                {
                    action = SaveAction.Delete;
                    OnSaving(this, action);
                    saveSucceeded = DataDelete();
                }
            }
            else
            {
                if (this.IsNew)
                {
                    if (_DateCreated == DateTime.MinValue)
                        _DateCreated = DateTime.Now;

                    _DateModified = null;
                    action = SaveAction.Insert;
                    OnSaving(this, action);
                    saveSucceeded = DataInsert();
                }
                else
                {
                    this._DateModified = DateTime.Now; ;
                    action = SaveAction.Update;
                    OnSaving(this, action);
                    saveSucceeded = DataUpdate();
                }

                MarkOld();
            }

            OnSaved(this, action, saveSucceeded);
            return action;
        }

        #endregion

        #region Static Data access

        /// <summary>
        /// Retrieves the object from the data store and populates it.
        /// </summary>
        /// <param name="id">The unique identifier of the object.</param>
        /// <returns>True if the object exists and is being populated successfully</returns>
        protected abstract TYPE DataSelect(KEY id);

        /// <summary>
        /// Get data All
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //protected abstract TYPE DataSelectAll(KEY id);

        /// <summary>
        /// Updates the object in its data store.
        /// </summary>
        protected abstract bool DataUpdate();

        /// <summary>
        /// Inserts a new object to the data store.
        /// </summary>
        protected abstract bool DataInsert();

        /// <summary>
        /// Deletes the object from the data store.
        /// </summary>
        protected abstract bool DataDelete();

        #endregion

        #region Validation

        private StringDictionary _BrokenRules = new StringDictionary();

        /// <summary>
        /// Add or remove a broken rule.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="errorMessage">The description of the error</param>
        /// <param name="isBroken">True if the validation rule is broken.</param>
        protected virtual void AddRule(string propertyName, string errorMessage, bool isBroken)
        {
            if (isBroken)
            {
                _BrokenRules[propertyName] = errorMessage;
            }
            else
            {
                if (_BrokenRules.ContainsKey(propertyName))
                {
                    _BrokenRules.Remove(propertyName);
                }
            }
        }

        /// <summary>
        /// Reinforces the business rules by adding additional rules to the 
        /// broken rules collection.
        /// </summary>
        protected abstract void ValidationRules();


        /// /// <summary>
        /// If the object has broken business rules, use this property to get access
        /// to the different validation messages.
        /// </summary>
        public virtual string ValidationMessage
        {
            get
            {
                if (!IsValid)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string messages in this._BrokenRules.Values)
                    {
                        sb.AppendLine(messages);
                    }

                    return sb.ToString();
                }

                return string.Empty;
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when the class is Saved
        /// </summary>
        public event EventHandler<SavedEventArgs> Saved;
        /// <summary>
        /// Raises the Saved event.
        /// </summary>
        protected void OnSaved(EntityBase<TYPE, KEY> businessObject, SaveAction action, bool savedStatus)
        {
            if (Saved != null)
            {
                Saved(businessObject, new SavedEventArgs(action, savedStatus));
            }
        }

        /// <summary>
        /// Occurs when the class is Saved
        /// </summary>
        public event EventHandler<SavedEventArgs> Saving;
        /// <summary>
        /// Raises the Saving event
        /// </summary>
        protected void OnSaving(EntityBase<TYPE, KEY> businessObject, SaveAction action)
        {
            if (Saving != null)
            {
                Saving(businessObject, new SavedEventArgs(action, false));
            }
        }

        /// <summary>
        /// Occurs when this instance is marked dirty. 
        /// It means the instance has been changed but not saved.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises the PropertyChanged event safely.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IDisposable

        private bool _IsDisposed;
        /// <summary>
        /// Gets or sets if the object has been disposed.
        /// <remarks>
        /// If the objects is disposed, it must not be disposed a second
        /// time. The IsDisposed property is set the first time the object
        /// is disposed. If the IsDisposed property is true, then the Dispose()
        /// method will not dispose again. This help not to prolong the object's
        /// life if the Garbage Collector.
        /// </remarks>
        /// </summary>
        protected bool IsDisposed
        {
            get { return _IsDisposed; }
        }

        /// <summary>
        /// Disposes the object and frees ressources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">If true, the object gets disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed)
                return;

            if (disposing)
            {
                _ChangedProperties.Clear();
                _BrokenRules.Clear();
                _IsDisposed = true;
            }
        }

        /// <summary>
        /// Disposes the object and frees ressources for the Garbage Collector.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Equality overrides

        /// <summary>
        /// A uniquely key to identify this particullar instance of the class
        /// </summary>
        /// <returns>A unique integer value</returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Comapares this object with another
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the two objects as equal</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() == this.GetType())
            {
                return obj.GetHashCode() == this.GetHashCode();
            }

            return false;
        }

        /// <summary>
        /// Checks to see if two business objects are the same.
        /// </summary>
        public static bool operator ==(EntityBase<TYPE, KEY> first, EntityBase<TYPE, KEY> second)
        {
            if (Object.ReferenceEquals(first, second))
            {
                return true;
            }

            if ((object)first == null || (object)second == null)
            {
                return false;
            }

            return first.GetHashCode() == second.GetHashCode();
        }

        /// <summary>
        /// Checks to see if two business objects are different.
        /// </summary>
        public static bool operator !=(EntityBase<TYPE, KEY> first, EntityBase<TYPE, KEY> second)
        {
            return !(first == second);
        }

        #endregion

        #region IDataErrorInfo Members

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>An error message indicating what is wrong with this object. The default is an empty string ("").</returns>
        public string Error
        {
            get { return ValidationMessage; }
        }

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified column name.
        /// </summary>
        public string this[string columnName]
        {
            get
            {
                if (_BrokenRules.ContainsKey(columnName))
                    return _BrokenRules[columnName];

                return string.Empty;
            }
        }

        #endregion

        #region IChangeTracking Members

        /// <summary>
        /// Resets the object’s state to unchanged by accepting the modifications.
        /// </summary>
        void IChangeTracking.AcceptChanges()
        {
            Save();
        }

        #endregion
    }
}
