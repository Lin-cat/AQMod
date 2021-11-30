﻿using AQMod.Content.Quest.Lobster.HuntTypes;

namespace AQMod.Content.Quest.Lobster
{
    public class RobsterHuntLoader : ContentLoader<RobsterHunt>
    {
        public override void Setup(bool setupStatics = false)
        {
            base.Setup(setupStatics);
            var mod = AQMod.Instance;
            AddContent(new HuntJeweledChalice(mod, "JeweledChalice"));
            AddContent(new HuntJeweledCandelabra(mod, "JeweledCandelabra"));
        }

        /// <summary>
        /// Runs the setup method for each hunt
        /// </summary>
        public void SetupHunts()
        {
            foreach (RobsterHunt h in _content)
            {
                h.Setup();
            }
        }
    }
}