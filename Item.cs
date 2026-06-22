using app_test.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace app_test
{
    //permet d'ajouter la variable type dans le JSON
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    //Permet de déclarer tout les types pris en charge,
    [JsonDerivedType(typeof(Texte), typeDiscriminator: "texte")]
    [JsonDerivedType(typeof(Items.Image), typeDiscriminator: "image")]
    [JsonDerivedType(typeof(Video), typeDiscriminator: "video")]
    [JsonDerivedType(typeof(Audio), typeDiscriminator: "audio")]
    [JsonDerivedType(typeof(Dessin), typeDiscriminator: "dessin")]
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Position { get; set; }
        public string Source { get; set; }

        public Item(int Position, string Source)
        {
            this.Position = Position;
            this.Source = Source;
        }

        public Item()
        {
            Position = 0;
            Source = "basique";
        }
    }
}
