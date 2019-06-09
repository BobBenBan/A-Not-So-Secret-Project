using System.Collections.Generic;
using MusicMachine.Programs;
using MusicMachine.Programs.Mappers;
using MusicMachine.Util;

namespace MusicMachine.Mechanisms.Projectiles
{
public delegate IEnumerable<Pair<long, LaunchInfo>> TrackToLaunchMapper(AnyTrack track, MappingInfo mappingInfo);
}