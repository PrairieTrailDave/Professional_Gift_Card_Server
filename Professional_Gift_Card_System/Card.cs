//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Professional_Gift_Card_System
{
    using System;
    using System.Collections.Generic;
    
    public partial class Card
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Card()
        {
            this.Histories = new HashSet<History>();
            this.Histories1 = new HashSet<History>();
        }
    
        public int ID { get; set; }
        public System.Guid CardGUID { get; set; }
        public string CardNumber { get; set; }
        public string CardNumLast4 { get; set; }
        public Nullable<System.Guid> MerchantGUID { get; set; }
        public Nullable<System.Guid> CardHolderGUID { get; set; }
        public string Shipped { get; set; }
        public string Activated { get; set; }
        public decimal GiftBalance { get; set; }
        public int LoyaltyBalance { get; set; }
        public Nullable<System.DateTime> DateShipped { get; set; }
        public Nullable<System.DateTime> DateActivated { get; set; }
    
        public virtual CardHolder CardHolder { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<History> Histories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<History> Histories1 { get; set; }
        public virtual Merchant Merchant { get; set; }
    }
}
