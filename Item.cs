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
        public string Source { get; set; }

        public double? Largeur { get; set; }
        public double? Hauteur { get; set; }

        public Item(string Source)
        {
            this.Source = Source;
        }

        public Item()
        {
            Source = "basique";
        }
    }
}
