namespace FluidTrade.Guardian.Windows
{

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;

	using Type = System.Collections.Generic.KeyValuePair<System.String, System.Guid>;
	using FluidTrade.Core;
	using System.Windows.Threading;
	using FluidTrade.Guardian.AdminSupportReference;
	using FluidTrade.Guardian.TradingSupportReference;

	/// <summary>
	/// Interaction logic for WindowAddOrganization.xaml
	/// </summary>
	public partial class WindowAddOrganization : Window
	{

		/// <summary>
		/// Indicates the OrganizationName dependency property.
		/// </summary>
		public static readonly DependencyProperty OrganizationNameProperty = DependencyProperty.Register("OrganizationName", typeof(String), typeof(WindowAddOrganization));
		/// <summary>
		/// Indicates the OrganizationType dependency property.
		/// </summary>
		public static readonly DependencyProperty OrganizationTypeProperty = DependencyProperty.Register("OrganizationType", typeof(Guid), typeof(WindowAddOrganization));
		/// <summary>
		/// Indicates the Types dependency property.
		/// </summary>
		public static readonly DependencyProperty TypesProperty = DependencyProperty.Register("Types", typeof(List<Type>), typeof(WindowAddOrganization));

		/// <summary>
		/// Create a new Add organization window.
		/// </summary>
		public WindowAddOrganization()
		{
			InitializeComponent();

			this.Loaded += OnLoad;
		}

		/// <summary>
		/// The new organization's name.
		/// </summary>
		public String OrganizationName
		{
			get { return this.GetValue(WindowAddOrganization.OrganizationNameProperty) as String; }
			set { this.SetValue(WindowAddOrganization.OrganizationNameProperty, value); }
		}

		/// <summary>
		/// The new organization's initiali entity's type.
		/// </summary>
		public Guid OrganizationType
		{
			get { return (Guid)this.GetValue(WindowAddOrganization.OrganizationTypeProperty); }
			set { this.SetValue(WindowAddOrganization.OrganizationTypeProperty, value); }
		}

		/// <summary>
		/// The types available for the initial entity.
		/// </summary>
		public List<Type> Types
		{
			get { return this.GetValue(WindowAddOrganization.TypesProperty) as List<Type>; }
			set { this.SetValue(WindowAddOrganization.TypesProperty, value); }
		}

		private void OnLoad(object sender, EventArgs eventArgs)
		{

			ThreadPoolHelper.QueueUserWorkItem(this.LoadTypes);

		}

		private void LoadTypes(object data)
		{

			lock (DataModel.SyncRoot)
			{

				List<Type> types = new List<Type>();

				TypeRow debtHolder = DataModel.Type.TypeKeyExternalId0.Find("DEBT HOLDER");
				TypeRow debtSettler = DataModel.Type.TypeKeyExternalId0.Find("DEBT NEGOTIATOR");

				types.Add(new Type(debtHolder.Description, debtHolder.TypeId));
				types.Add(new Type(debtSettler.Description, debtSettler.TypeId));

				this.Dispatcher.BeginInvoke(
					new Action(delegate()
						{
							this.Types = types;
							this.OrganizationType = types[0].Value;
						}),
					DispatcherPriority.Normal);

			}

		}

		private void OnCancel(object sender, EventArgs eventArgs)
		{

			this.Close();

		}

		private void OnOkay(object sender, EventArgs eventArgs)
		{

			String name = this.OrganizationName;
			Guid typeId = this.OrganizationType;

			ThreadPoolHelper.QueueUserWorkItem(
				data =>
					this.Create(name, typeId));

			this.Close();

		}

		private void Create(String name, Guid typeId)
		{

			AdminSupportClient adminClient = new AdminSupportClient(Properties.Settings.Default.AdminSupportEndpoint);
			Guid organizationId = Guid.Empty;

			try
			{

				AdminSupportReference.MethodResponseguid response;
				String currentOrganization;

				lock (DataModel.SyncRoot)
					currentOrganization = DataModel.User.UserKey.Find(UserContext.Instance.UserId).TenantRow.ExternalId0;

				response = adminClient.AddOrganization(name, currentOrganization);
				adminClient.Close();

				if (!response.IsSuccessful)
				{

					if (response.Errors.Length > 0)
						GuardianObject.ThrowErrorInfo(response.Errors[0]);
					else
						throw new Exception("Unknown error occured");
				
				}
				else
				{

					organizationId = response.Result;

				}

			}
			catch
			{

				MessageBox.Show(Properties.Resources.OperationFailed);
				return;

			}

			try
			{

				Guid parentId;

				lock (DataModel.SyncRoot)
					parentId = FindUserFolder();

				Guid entityId = Entity.Create(typeId, parentId, organizationId);
				TradingSupportReference.TradingSupportClient client = new TradingSupportReference.TradingSupportClient(Properties.Settings.Default.TradingSupportEndpoint);
				client.UpdateEntity(new TradingSupportReference.Entity[] { new TradingSupportReference.Entity() { RowId = entityId, Name = name } });

			}
			catch
			{

				MessageBox.Show(Properties.Resources.OperationFailed);
				return;

			}

		}

		private Guid FindUserFolder()
		{

			EntityRow userEntityRow = DataModel.Entity.EntityKey.Find(UserContext.Instance.UserId);
			EntityTreeRow[] children = userEntityRow.GetEntityTreeRowsByFK_Entity_EntityTree_ParentId();

			foreach (EntityTreeRow entityTreeRow in children)
			{

				EntityRow folderEntityRow = entityTreeRow.EntityRowByFK_Entity_EntityTree_ChildId;

				if (folderEntityRow.TypeRow.ExternalId0.Equals("SYSTEM FOLDER"))
					return folderEntityRow.EntityId;

			}

			throw new ArgumentException("Current user has no system folder");

		}
	
	}

}
