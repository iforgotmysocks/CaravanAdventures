using RimWorld.Planet;

namespace CaravanAdventures.CaravanCamp
{
    public interface IRecipeHolder
    {
        void ApplyRecipes(Caravan caravan);
        void ApplyRecipesTribal(Caravan caravan);
    }
}
