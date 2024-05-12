import { atom, useRecoilValue } from "recoil"
import ApiRequest from "../ApiRequest"
import { Guild } from "../Models"

async function loadAllGuilds() {
  const response = await ApiRequest("GET", `guild`)
  const guilds = (await response.json()) as Guild[]

  return guilds
}

export const allGuildsAtom = atom<Guild[]>({
  key: "allGuilds",
  effects: [({ setSelf }) => setSelf(loadAllGuilds())],
})

export function useGuilds() {
  const guilds = useRecoilValue(allGuildsAtom)

  return guilds
}

export function useGuild(guildId: string) {
  const guilds = useRecoilValue(allGuildsAtom)
  const guild = guilds.find((guild) => guild.id == guildId) ?? null

  return guild
}
