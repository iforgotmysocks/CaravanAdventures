using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaravanAdventures.CaravanCamp
{
    interface IRecipeHolder
    {
        void ApplyRecipes(Caravan caravan);

        void ApplyRecipesTribal(Caravan caravan);
    }
}
