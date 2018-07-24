namespace FluidTrade.Actipro
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Media.Imaging;
    using System.IO;
    using System.Windows.Media;

    /// <summary>
    /// 
    /// </summary>
    public class EntityNode
    {
        /// <summary>
        /// 
        /// </summary>
        public String Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public String ImageData { get; set; }

        /// <summary>
        /// Defualt Constructor
        /// </summary>
        public EntityNode()
		{

			// Initialize the object.
			
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
                if (String.IsNullOrEmpty(this.ImageData))
                    return null;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = new MemoryStream(Convert.FromBase64String(this.ImageData));
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Copy(EntityNode entity)
        {

            // Note that the properties are used to copy the values.  This will trigger update events for any listeners.
            this.Name = entity.Name;            
            this.ImageData = entity.ImageData;            
            this.EntityId = entity.EntityId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.EntityId.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override String ToString() 
        { 
            return this.Name; 
        }
    }
}
