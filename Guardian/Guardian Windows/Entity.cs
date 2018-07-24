namespace FluidTrade.Guardian.Windows
{


	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.ServiceModel;
	using System.ServiceModel.Security;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Threading;
	using FluidTrade.Core;
	using FluidTrade.Core.Windows.Controls;
	using FluidTrade.Guardian.TradingSupportReference;
	using System.Data.SqlClient;
	using System.Data.Common;

	/// <summary>
	/// Summary description for Object.
	/// </summary>
	public class Entity : GuardianObject
	{

		// Private Instance Fields
		private DateTime createdTime;
		private String description;
		private Guid entityId;
		private Boolean finishedLoading;
		private Guid imageId;
		private String imageData;
		private Boolean isHidden;
		private Boolean isReadOnly;
		private DateTime modifiedTime;
		private String name;
		private Guid tenantId;
        private Guid typeId;
		private String typeName;
		private String viewerType;
		private Int64 rowVersion;

		private Window propertiesWindow;

        /// <summary>
        /// Create an empty entity.
        /// </summary>
		public Entity()
		{

			// Initialize the object.
			this.createdTime = DateTime.Now;
			this.entityId = Guid.Empty;
			this.propertiesWindow = null;
			this.Modified = true;
			this.modifiedTime = this.createdTime;
			this.finishedLoading = true;

		}

		/// <summary>
		/// Create an entity.
		/// </summary>
		/// <param name="entityRow">The description of the entity.</param>
		public Entity(EntityRow entityRow)
		{

			// Initialize the object
			this.createdTime = entityRow.CreatedTime.ToLocalTime();
			this.description = entityRow.IsDescriptionNull() ? String.Empty : entityRow.Description;
			this.entityId = entityRow.EntityId;
			this.modifiedTime = entityRow.ModifiedTime.ToLocalTime();
			this.name = entityRow.Name;
			this.imageId = entityRow.ImageId;
			this.imageData = entityRow.ImageRow.Image;
			this.isReadOnly = entityRow.IsReadOnly;
			this.isHidden = entityRow.IsHidden;
			this.rowVersion = entityRow.RowVersion;
			this.tenantId = entityRow.TenantId;
			this.typeId = entityRow.TypeId;			
			this.typeName = entityRow.TypeRow.Description;
			this.viewerType = entityRow.TypeRow.ViewerType;
			this.propertiesWindow = null;

			if (this.NeedLateLoad())
			{

				this.finishedLoading = false;
				DataModel.EndMerge += this.FinishLateLoad;

			}
			else
			{

				this.finishedLoading = true;
				this.FinishLoad();

			}

		}

		/// <summary>
		/// Create a duplicate entity.
		/// </summary>
		/// <param name="source">The original entity.</param>
		public Entity(Entity source)
		{

			this.createdTime = source.createdTime;
			this.description = source.Description;
			this.entityId = source.EntityId;
			this.modifiedTime = source.modifiedTime;
			this.name = source.Name;
			this.imageId = source.ImageId;
			this.imageData = source.ImageData;
			this.isReadOnly = source.IsReadOnly;
			this.isHidden = source.IsHidden;
			this.rowVersion = source.RowVersion;
			this.tenantId = source.tenantId;
			this.typeId = source.TypeId;
			this.typeName = source.TypeName;
			this.viewerType = source.viewerType;
			this.propertiesWindow = null;
			this.finishedLoading = true;

		}

		/// <summary>
		/// The time the object was created.
		/// </summary>
		public DateTime CreatedTime
		{

			get { return this.createdTime; }
			private set
			{
				if (this.createdTime != value)
				{
					this.createdTime = value;
					OnPropertyChanged(new PropertyChangedEventArgs("CreatedTime"));
				}
			}

		}

		/// <summary>
		/// Gets or sets the description of the object.
		/// </summary>
		public String Description
		{
			get { return this.description; }
			set
			{
				if (this.description != value)
				{
					this.description = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Description"));
				}
			}
		}

		/// <summary>
		/// Gets the unique identifier of the entity.
		/// </summary>
		public Guid EntityId
		{
			get { return this.entityId; }
		}


		/// <summary>
		/// Whether the whole object is loaded.
		/// </summary>
		public Boolean FinishedLoading
		{

			get { return this.finishedLoading; }
			private set
			{

				if (this.finishedLoading != value)
				{

					this.finishedLoading = value;
					OnPropertyChanged(new PropertyChangedEventArgs("FinishedLoading"));

				}

			}

		}

		/// <summary>
		/// Gets or sets the imageId for this object.
		/// </summary>
		public Guid ImageId
		{
			get { return this.imageId; }
			set
			{
				if (this.imageId != value)
				{
					this.imageId = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ImageId"));
				}
			}
		}

		/// <summary>
		/// Gets or sets the raw image data for this object.
		/// </summary>
		public String ImageData
		{
			get { return this.imageData; }
			set
			{
				if (this.imageData != value)
				{
					this.imageData = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ImageData"));
					OnPropertyChanged(new PropertyChangedEventArgs("ImageSource"));
				}
			}
		}

		/// <summary>
		/// Gets the ImageSource used to display an image of this object.
		/// </summary>
		public ImageSource ImageSource
		{
			get
			{
				// The images are created on demand.  There many be hundreds of objects in a tree and an image isn't required until
				// one of them is actually displayed.
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(this.imageData));
				bitmapImage.EndInit();
				return bitmapImage;
			}
		}

		/// <summary>
		/// Gets or sets the whether the entity is hidden or visible.
		/// </summary>
		public Boolean IsHidden
		{
			get { return this.isHidden; }
			set
			{
				if (this.isHidden != value)
				{
					this.isHidden = value;
					OnPropertyChanged(new PropertyChangedEventArgs("IsHidden"));
				}
			}
		}

		/// <summary>
		/// Gets or sets the whether the entity allows for writing.
		/// </summary>
		public Boolean IsReadOnly
		{
			get { return this.isReadOnly; }
			set
			{
				if (this.isReadOnly != value)
				{
					this.isReadOnly = value;
					OnPropertyChanged(new PropertyChangedEventArgs("IsReadOnly"));
				}
			}
		}

		/// <summary>
		/// The last time the object was modified.
		/// </summary>
		public DateTime ModifiedTime
		{

			get { return this.modifiedTime; }
			private set
			{
				if (this.modifiedTime != value)
				{
					this.modifiedTime = value;
					OnPropertyChanged(new PropertyChangedEventArgs("ModifiedTime"));
				}
			}

		}

		/// <summary>
		/// Gets or sets the name of the object.
		/// </summary>
		public String Name
		{
			get { return this.name; }
			set
			{
				if (this.name != value)
				{
					this.name = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Name"));
				}
			}
		}

		/// <summary>
		/// Gets the entity type of this object.
		/// </summary>
		public Guid TenantId
		{
			get { return this.tenantId; }
			set
			{
				if (this.tenantId != value)
				{
					this.tenantId = value;
					OnPropertyChanged(new PropertyChangedEventArgs("TenantId"));
				}
			}
		}


        /// <summary>
        /// Gets the entity type of this object.
        /// </summary>
        public Guid TypeId
        {
            get { return this.typeId; }
        }

		/// <summary>
		/// Gets the name of the entity type of this object.
		/// </summary>
		public override String TypeName
		{
			get { return this.typeName; }
		}

		/// <summary>
		/// The row version the information in this Entity corresponds to.
		/// </summary>
        public Int64 RowVersion
        {
            get { return this.rowVersion; }
        }

		/// <summary>
		/// Copies the data from one entity to another.
		/// </summary>
		/// <param name="obj">The source entity.</param>
		public override void Copy(GuardianObject obj)
		{

			Entity entity = obj as Entity;

			base.Copy(entity);

			if (this.RowVersion != entity.RowVersion)
			{

				// Note that the properties are used to copy the values.  This will trigger update events for any listeners.
				this.CreatedTime = entity.createdTime;
				this.Description = entity.description;
				this.entityId = entity.entityId;
				this.ModifiedTime = entity.modifiedTime;
				this.Name = entity.Name;
				this.ImageData = entity.imageData;
				this.IsHidden = entity.isHidden;
				this.IsReadOnly = entity.isReadOnly;
				this.rowVersion = entity.rowVersion;
				this.typeId = entity.TypeId;
				this.typeName = entity.TypeName;
				this.viewerType = entity.viewerType;

			}

		}

		/// <summary>
		/// Commit any changes to this object to the server.
		/// </summary>
		public override void Commit()
		{

			TradingSupportClient tradingSupportClient = new TradingSupportClient(Guardian.Properties.Settings.Default.TradingSupportEndpoint);

			try
			{

				TradingSupportReference.Entity record = new TradingSupportReference.Entity();
				MethodResponseErrorCode response;

				this.PopulateRecord(record);

				if (this.Deleted)
					throw new NotImplementedException("Cannot delete from Entity.Commit. You must override Commit in a derived class");
				else
					response = tradingSupportClient.UpdateEntity(new TradingSupportReference.Entity[] { record });

				if (!response.IsSuccessful)
					Entity.ThrowErrorInfo(response.Errors[0]);

				this.Modified = false;

			}
			catch (Exception exception)
			{

				// Any issues trying to communicate to the server are logged.
				EventLog.Error("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				throw;

			}
			finally
			{

				if (tradingSupportClient != null && tradingSupportClient.State == CommunicationState.Opened)
					tradingSupportClient.Close();
			}

		}

		/// <summary>
		/// Create a new entity of a given type under a particular parent entity.
		/// </summary>
		/// <param name="typeId">The type-id of the new entity.</param>
		/// <param name="tenantId"></param>
		/// <param name="parentId">The entity-id of the parent entity.</param>
		public static Guid Create(Guid typeId, Guid parentId, Guid tenantId)
		{

			lock (DataModel.SyncRoot)
			{

				TypeRow type = DataModel.Type.TypeKey.Find(typeId);
				Assembly assembly;
				String className;

				try
				{

					Entity.LoadAssembly(type.Type, out assembly, out className);
					Type entityType = assembly.GetType(className, true);

					MethodInfo create = entityType.GetMethod("Create", new Type[] { typeof(DataModelClient), typeof(Guid), typeof(Guid), typeof(Guid) });

					if (create != null)
					{

						DataModelClient dataModel = new DataModelClient(Properties.Settings.Default.DataModelEndpoint);
						// Create the entity and add it to the tree.
						Guid entityId = (Guid)create.Invoke(null, new object[] { dataModel, typeId, parentId, tenantId });
						//dataModel.CreateEntityTree(entityId, Guid.NewGuid(), null, parentId); ;
						//AccessRightRow accessRightRow = DataModel.AccessRight.AccessRightKeyAccessRightCode.Find(AccessRight.FullControl);
						//dataModel.CreateAccessControl(Guid.NewGuid(), accessRightRow.AccessRightId, entityId, Information.UserId);

						dataModel.Close();

						return entityId;

					}
					else
					{

						throw new Exception(string.Format("Create method not found in type '{0}'", className));

					}

				}
				catch (Exception exception)
				{

					throw new Exception(String.Format("Can't create new entity for type '{0}'", type.Description), exception);

				}

			}

		}

        /// <summary>
        /// Create a new entity in the data model.
        /// </summary>
        /// <param name="dataModel">The data model client to create the entity with.</param>
        /// <param name="typeId">The type of the new entity.</param>
		/// <param name="parentId">The entityId of the parent entity.</param>
		/// <param name="tenantId"></param>
        /// <returns>The entity Id of the new entity.</returns>
        public static Guid Create(DataModelClient dataModel, Guid typeId, Guid parentId, Guid tenantId)
        {

            Guid entityId = Guid.NewGuid();
			Guid imageId;

			lock (DataModel.SyncRoot)
			{

				TypeRow type = DataModel.Type.TypeKey.Find(typeId);
				imageId = type.IsImageIdNull() ? DataModel.Image[0].ImageId : type.ImageId; ;

			}

			dataModel.CreateEntity(DateTime.UtcNow, "", entityId,
                null, null, null, null, null, null, null, null,
				imageId, false, false, DateTime.UtcNow, "New Entity", tenantId, typeId);

            return entityId;

        }

		/// <summary>
		/// Create a new via for this entity of the appropriate type.
		/// </summary>
		/// <param name="arguments">An array of arguments to pass to the viewer's constructor</param>
		/// <returns>The newly created viewer.</returns>
		public Viewer CreateViewer(params Object[] arguments)
		{

			// The tree node contains a specification for the page that can handle this type of object.  The is loaded up 
			// dynamically from that specification.
            Assembly assembly;
            String className;

            try
            {

                Entity.LoadAssembly(this.viewerType, out assembly, out className);

            }
            catch (Exception exception)
            {

                throw new Exception(string.Format("Can't create an viewer '{0}' for type {1}", this.viewerType, this.GetType()), exception);

            }

			// Combine the destination object and the optional arguments into a single array.
			Object[] combinedArguments = null;
			combinedArguments = new Object[1 + arguments.Length];
			combinedArguments[0] = this;
			Array.Copy(arguments, 0, combinedArguments, 1, arguments.Length);

			// This will dynamically create a page from the type information found in the selected object.  The EntityId is used universally to uniquely
			// identify a navigatable object.
			return assembly.CreateInstance(
				className,
				false,
				BindingFlags.CreateInstance,
				null,
				combinedArguments,
				System.Globalization.CultureInfo.CurrentCulture,
				null) as Viewer;

		}

		/// <summary>
		/// Create the properties window appropriate for this type.
		/// </summary>
		/// <returns></returns>
		protected virtual WindowEntityProperties CreatePropertiesWindow()
		{

			return new WindowEntityProperties();

		}

		/// <summary>
		/// Determin whether this object is equal to another.
		/// </summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the other object is also an Entity and has the same EntityId as this Entity.</returns>
		public override Boolean Equals(Object obj)
		{

			if (obj is Entity)
			{
				Entity guardianObject = obj as Entity;
				return this.entityId == guardianObject.entityId;
			}

			return false;

		}

		/// <summary>
		/// Try to finish a delayed load of the object.
		/// </summary>
		/// <param name="sender">The data model.</param>
		/// <param name="eventArgs">The event arguments.</param>
		private void FinishLateLoad(object sender, EventArgs eventArgs)
		{

			if (!this.NeedLateLoad())
			{

				Entity newEntity;

				DataModel.EndMerge -= this.FinishLateLoad;

				newEntity = Entity.New(DataModel.Entity.EntityKey.Find(this.EntityId));

				Application.Current.Dispatcher.BeginInvoke(
					new Action(delegate()
						{
							
							this.Copy(newEntity);
							this.FinishedLoading = true;

						})
					, DispatcherPriority.Normal);

			}

		}

		/// <summary>
		/// Finish loading whatever rows we were waiting on.
		/// </summary>
		protected virtual void FinishLoad()
		{


		}

		/// <summary>
		/// Retrieve the custom menu items for this entity.
		/// </summary>
		/// <returns>The list of custom menu items.</returns>
		public virtual List<Control> GetCustomMenuItems()
		{

			return new List<Control>();

		}

		/// <summary>
		/// Get the hash code that identifies this Entity.
		/// </summary>
		/// <returns>The hash code of this entity.</returns>
		public override int GetHashCode()
		{
			return this.entityId.GetHashCode();
		}

		/// <summary>
		/// Load an assembly from based on a type description.
		/// </summary>
		/// <param name="typeName">The description of the type, eg. "FluidTrade.Guardian.Blotter, Guardian Windows"</param>
		/// <param name="assembly">The loaded assembly containing the specified type.</param>
		/// <param name="className">The name of the type in the description.</param>
		public static void LoadAssembly(String typeName, out Assembly assembly, out String className)
		{

			String[] typeParts = typeName.Split(',');
			if (typeParts.Length < 2)
				throw new Exception(string.Format("Type name '{0}' is incorrectly formatted", typeName));
			String assemblyName = typeParts[1].Trim();

			for (int index = 2; index < typeParts.Length; index++)
				assemblyName += ',' + typeParts[index].Trim();

			//try
			//{
				assembly = Assembly.Load(assemblyName);
				className = typeParts[0].Trim();
			//}
			//catch
			//{
			//    String[] files = Directory.GetFiles(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]), "*.dll");

			//    foreach(string file in files)
			//    {

			//        Assembly ass1 = Assembly.ReflectionOnlyLoadFrom(file);
			//        System.Diagnostics.Trace.WriteLine(ass1.FullName);
			//    }

			//    throw;
			//}
		}

		/// <summary>
		/// Determine whether we need to wait for data model to finish updating before building object is completed.
		/// </summary>
		/// <returns></returns>
		protected virtual Boolean NeedLateLoad()
		{

			return false;

		}

		/// <summary>
		/// Create a new Entity object from an EntityRow from the DataModel.
		/// </summary>
		/// <param name="entityRow">The entity row to base the entity object from.</param>
		/// <returns>The newly populated Entity object.</returns>
		public static Entity New(EntityRow entityRow)
		{

			String[] typeParts = entityRow.TypeRow.Type.Split(',');
			Assembly assembly;
			string className;
			
			Entity.LoadAssembly(entityRow.TypeRow.Type, out assembly, out className);
			return assembly.CreateInstance(className, false, BindingFlags.CreateInstance, null,
				new object[] { entityRow }, CultureInfo.InvariantCulture, null) as Entity;   
			
		}
		/// <summary>
		/// Create a new Entity object from an TypeRow from the DataModel.
		/// </summary>
		/// <param name="typeRow">The type row to base the entity object from.</param>
		/// <returns>The newly populated Entity object.</returns>
		public static Entity New(TypeRow typeRow)
		{

			String[] typeParts = typeRow.Type.Split(',');
			Assembly assembly;
			string className;

			Entity.LoadAssembly(typeRow.Type, out assembly, out className);
			return assembly.CreateInstance(className, false, BindingFlags.CreateInstance, null,
				new object[0], CultureInfo.InvariantCulture, null) as Entity;

		}

		/// <summary>
		/// Populates a trading support record with the contents of the entity.
		/// </summary>
		/// <param name="baseRecord">The empty record to populate.</param>
		protected override void PopulateRecord(TradingSupportReference.BaseRecord baseRecord)
		{

			TradingSupportReference.Entity record = baseRecord as TradingSupportReference.Entity;

			record.Description = this.Description;
			record.ImageId = this.ImageId;
			record.IsHidden = this.IsHidden;
			record.IsReadOnly = this.IsReadOnly;
			record.Name = this.Name;
			record.RowId = this.EntityId;
			record.RowVersion = this.RowVersion;
			record.TypeId = this.TypeId;

		}

		/// <summary>
		/// Determine if an Entity object is equal to another object.
		/// </summary>
		/// <param name="object1">The entity.</param>
		/// <param name="object2">The other object.</param>
		/// <returns>True if the both are null or they are Equal(). False otherwise.</returns>
		public static Boolean operator ==(FluidTrade.Guardian.Windows.Entity object1, object object2)
		{

			if (Object.ReferenceEquals(object1, null) && Object.ReferenceEquals(object2, null))
				return true;

			if (Object.ReferenceEquals(object1, null) || Object.ReferenceEquals(object2, null))
				return false;

			return object1.Equals(object2);

		}

		/// <summary>
		/// Determin if an Entity object is not equal to another object.
		/// </summary>
		/// <param name="object1">The entity.</param>
		/// <param name="object2">The other object.</param>
		/// <returns>False if they are bother null or they are not Equal(). False otherwise.</returns>
		public static Boolean operator !=(FluidTrade.Guardian.Windows.Entity object1, object object2)
		{

			if (Object.ReferenceEquals(object1, null) && Object.ReferenceEquals(object2, null))
				return false;

			if (Object.ReferenceEquals(object1, null) || Object.ReferenceEquals(object2, null))
				return true;

			return !object1.Equals(object2);

		}

		/// <summary>
		/// Show the currently open properties window.
		/// </summary>
		private void ShowCurrentProperties()
		{

			this.propertiesWindow.Activate();

		}

		/// <summary>
		/// Show the properties window for this entity. Launch in a different apartment and make sure there's only one for this entity.
		/// </summary>
		public void ShowProperties(FolderTreeNode rootNode)
		{

			try
			{

				if (this.propertiesWindow != null)
				{

					if (this.propertiesWindow != null)
						this.ShowCurrentProperties();

				}
				else
				{

					Entity entity = this.Clone() as Entity;
					Guid root = rootNode == null? Guid.Empty : rootNode.Entity.EntityId;

					this.ShowProperties(root, entity);

				}

			}
			catch (Exception exception)
			{

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
				throw;

			}

		}

		/// <summary>
		/// Show the properties window for this entity.
		/// </summary>
		/// <param name="root">The entityId of the root of the tree.</param>
		/// <param name="entity">The a clone of ourself.</param>
		private void ShowProperties(Guid root, Entity entity)
		{

			WindowEntityProperties properties = this.CreatePropertiesWindow();

			this.propertiesWindow = properties;


			try
			{

				properties.Entity = entity;
				properties.SetTreeRoot(root);

				if (!properties.IsVisible)
				{

					properties.Closed += (s, e) => this.propertiesWindow = null;
					properties.Show();

				}

			}
			catch (Exception exception)
			{

				this.propertiesWindow = null;

				EventLog.Warning("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);

				MessageBox.Show(Application.Current.MainWindow, Properties.Resources.OperationFailed, Application.Current.MainWindow.Name);

			}

		}

		/// <summary>
		/// Update the Entity based on another Entity.
		/// </summary>
		/// <param name="obj">The Entity to update from.</param>
		public override void Update(GuardianObject obj)
		{

			Entity entity = obj as Entity;

			if (!this.Modified && entity.EntityId == this.EntityId)
			{

				this.CreatedTime = entity.createdTime;
				this.Description = entity.Description;
				this.ModifiedTime = entity.modifiedTime;
				this.Name = entity.Name;
				this.ImageId = entity.ImageId;
				this.ImageData = entity.ImageData;
				this.IsReadOnly = entity.IsReadOnly;
				this.IsHidden = entity.IsHidden;
				this.viewerType = entity.viewerType;
				this.typeId = entity.TypeId;
				this.typeName = entity.TypeName;
				this.Modified = false;

			}

			this.rowVersion = entity.RowVersion;

		}

		/// <summary>
		/// Update the Entity based on an EntityRow.
		/// </summary>
		/// <param name="entityRow">The row to update from.</param>
		public virtual void Update(EntityRow entityRow)
		{

			if (!this.Modified && entityRow.EntityId == this.EntityId)
			{

				this.CreatedTime = entityRow.CreatedTime.ToLocalTime();
				this.Description = entityRow.IsDescriptionNull() ? String.Empty : entityRow.Description;
				this.ModifiedTime = entityRow.ModifiedTime.ToLocalTime();
				this.Name = entityRow.Name;
				this.ImageId = entityRow.ImageId;
				this.ImageData = entityRow.ImageRow.Image;
				this.IsReadOnly = entityRow.IsReadOnly;
				this.IsHidden = entityRow.IsHidden;
				this.viewerType = entityRow.TypeRow.ViewerType;
				this.typeId = entityRow.TypeId;
				this.typeName = entityRow.TypeRow.Description;
				this.Modified = false;

			}
		
			this.rowVersion = entityRow.RowVersion;

		}

		/// <summary>
		/// Get the Entity as a string.
		/// </summary>
		/// <returns>The entity's name.</returns>
		public override String ToString() { return this.Name; }

	}

}
