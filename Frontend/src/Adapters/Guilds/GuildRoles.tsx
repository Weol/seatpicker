import { atomFamily, useRecoilState } from "recoil"
import ApiRequest from "../ApiRequest"
import { Guild, GuildRole } from "./ActiveGuild"

async function LoadGuildRoles(guildId: string) {
  const response = await ApiRequest("GET", `guild/${guildId}/roles`)
  return (await response.json()) as GuildRole[]
}

export const guildRolesAtomFamily = atomFamily<GuildRole[], string>({
  key: "guildRoles",
  default: LoadGuildRoles,
})

export function useGuildRoles(guildId: string) {
  const [guildRoles, setGuildRoles] = useRecoilState(guildRolesAtomFamily(guildId))

  const updateGuildRoles = async (guild: Guild, guildRoles: GuildRole[]) => {
    await ApiRequest("PUT", `guild/${guild.id}/roles`, guildRoles)
    setGuildRoles(await LoadGuildRoles(guildId))
  }

  return { guildRoles, updateGuildRoles }
}
