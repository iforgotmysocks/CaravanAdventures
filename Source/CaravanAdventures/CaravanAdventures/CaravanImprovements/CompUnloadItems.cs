using Verse;

namespace CaravanAdventures.CaravanImprovements
{
    class CompUnloadItems : MapComponent
    {
        private bool unload = false;
        public bool Unload { get => unload; set => unload = value; }
        public CompUnloadItems(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref unload, "unload");
        }

    }
}
