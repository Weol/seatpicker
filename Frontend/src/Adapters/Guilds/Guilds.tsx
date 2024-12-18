import {atom, useRecoilRefresher_UNSTABLE, useRecoilValue} from "recoil"
import { NotFoundError } from "../AdapterError"
import ApiRequest from "../ApiRequest"
import {Guild, UnconfiguredGuild} from "../Models"

async function loadConfiguredGuilds() {
  const response = await ApiRequest("GET", `guild`)
  const guilds = (await response.json()) as Guild[]

  return guilds
}

async function loadUnconfiguredGuilds() {
  const response = await ApiRequest("GET", `guild/unconfigured`)
  return (await response.json()) as UnconfiguredGuild[]
}

export const configuredGuildsAtom = atom<Guild[]>({
  key: "allGuilds",
  effects: [({ setSelf }) => setSelf(loadConfiguredGuilds())],
})

export const unconfiguredGuildsAtom = atom<UnconfiguredGuild[]>({
  key: "allUnconfiguredGuilds",
  effects: [({ setSelf }) => setSelf(loadUnconfiguredGuilds())],
})

export function useGuilds() {
  const guilds = useRecoilValue(configuredGuildsAtom)

  return guilds
}

export function useGuild(guildId: string) {
  const guilds = useRecoilValue(configuredGuildsAtom)
  const guild = guilds.find((guild) => guild.id == guildId)
  if (!guild) throw new NotFoundError()

  return guild
}

export function useUnconfiguredGuilds() {
  const unconfiguredGuilds = useRecoilValue(unconfiguredGuildsAtom)

  const configureGuild = async (unconfiguredGuild: UnconfiguredGuild) => {
    await ApiRequest("PUT", `guild/${unconfiguredGuild.id}`, {
      id: unconfiguredGuild.id,
      name: unconfiguredGuild.name,
      icon: unconfiguredGuild.icon,
      hostNames: [],
      roleMapping: []
    })

    const refreshUnconfiguredGuilds = useRecoilRefresher_UNSTABLE(unconfiguredGuildsAtom);
    const refreshConfiguredGuilds = useRecoilRefresher_UNSTABLE(configuredGuildsAtom);
    refreshConfiguredGuilds()
    refreshUnconfiguredGuilds()
  }

  return { unconfiguredGuilds, configureGuild }
}
