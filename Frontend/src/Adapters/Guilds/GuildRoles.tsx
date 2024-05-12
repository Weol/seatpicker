import { atomFamily, useRecoilState } from "recoil"
import ApiRequest from "../ApiRequest"
import { Guild, GuildRole } from "../Models"

async function loadGuildRoles(guildId: string) {
  const response = await ApiRequest("GET", `guild/discord/${guildId}/roles`)
  return (await response.json()) as GuildRole[]
}

export const guildRolesAtomFamily = atomFamily<GuildRole[], string>({
  key: "guildRoles",
  default: loadGuildRoles,
})

export function useGuildRoles(guildId: string) {
  const [guildRoles, setGuildRoles] = useRecoilState(guildRolesAtomFamily(guildId))

  const updateGuildRoles = async (guild: Guild, guildRoles: GuildRole[]) => {
    await ApiRequest("PUT", `guild/discord/${guild.id}/roles`, guildRoles)
    setGuildRoles(await loadGuildRoles(guildId))
  }

  return { guildRoles, updateGuildRoles }
}
