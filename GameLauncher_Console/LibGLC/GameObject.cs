using System;
using System.Collections.Generic;

namespace LibGLC
{
    public class CGameObject
    {
        public CGameObject()
        {

        }

        public List<Game> GetByPlatform(string plaftorm)
        {
            List<Game> gameList = new List<Game>();
            for(int i = 0; i < 15; i++)
            {
                gameList.Add(new Game(string.Format("{0}: Game -> {1}", plaftorm, i)));
            }
            return gameList;
        }

        public Game GetByName(string name)
        {
            return new Game(name);
        }
    }

    public class Game
    {
        public string name;

        public Game(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
