using System.Collections.Generic;

namespace ZigdarkS.ProjectB.Enemy.Logic
{
    /// <summary>
    /// Синглтон-сервис (зарегистрируй в VContainer).
    /// Хранит все активные отряды, создаёт новые по запросу.
    /// </summary>
    public class EnemySquadRegistry
    {
        private readonly Dictionary<string, EnemySquad> _squads = new();

        public EnemySquad GetOrCreate(string squadId)
        {
            if (!_squads.TryGetValue(squadId, out var squad))
            {
                squad = new EnemySquad(squadId);
                _squads[squadId] = squad;
            }
            return squad;
        }

        public bool TryGet(string squadId, out EnemySquad squad)
        {
            return _squads.TryGetValue(squadId, out squad);
        }

        public void RemoveIfEmpty(string squadId)
        {
            if (_squads.TryGetValue(squadId, out var squad) && squad.AliveCount == 0)
                _squads.Remove(squadId);
        }
    }
}