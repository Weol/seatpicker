import { atom, useRecoilState, useSetRecoilState } from "recoil"
import { ApiRequest } from "../ApiRequest"
import { Guild, UnconfiguredGuild } from "../Models"
import { NotFoundError } from "../AdapterError"

async function loadConfiguredGuilds() {
  const response = await ApiRequest("GET", `guild`)
  return (await response.json()) as Guild[]
}

async function loadUnconfiguredGuilds() {
  const response = await ApiRequest("GET", `guild/unconfigured`)
  return await response.json() as UnconfiguredGuild[]
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
  const [ configuredGuilds, setGuilds ] = useRecoilState(configuredGuildsAtom)
  
  const updateGuild = async (guild: Guild) => {
    const response = await ApiRequest("PUT", `guild/${guild.id}`, {
      id: guild.id,
      name: guild.name,
      icon: guild.icon,
      hostNames: guild.hostnames,
      roleMapping: guild.roleMapping,
    })

    const updatedGuild = await response.json() as Guild;
    const filtered = configuredGuilds.filter(guild => guild.id !== updatedGuild.id)
    setGuilds([... filtered, updatedGuild])
  }

  return { configuredGuilds, updateGuild } 
}

export function useGuild(guildId: string) {
  const { configuredGuilds, updateGuild } = useGuilds();
  const guild = configuredGuilds.find(guild => guild.id == guildId)
  if (!guild) throw new NotFoundError("Guild not found")
  
  return { guild, updateGuild }
}

export function useUnconfiguredGuilds() {
  const [unconfiguredGuilds, setUnconfiguredGuilds] = useRecoilState(unconfiguredGuildsAtom)
  const setConfiguredGuilds = useSetRecoilState(configuredGuildsAtom)

  const configureGuild = async (unconfiguredGuild: UnconfiguredGuild) => {
    await ApiRequest("PUT", `guild/${unconfiguredGuild.id}`, {
      id: unconfiguredGuild.id,
      name: unconfiguredGuild.name,
      icon: unconfiguredGuild.icon,
      hostNames: [],
      roleMapping: [],
    })

    const configuredGuilds = loadConfiguredGuilds()
    const unconfiguredGuilds = loadUnconfiguredGuilds()
    setConfiguredGuilds(await configuredGuilds)
    setUnconfiguredGuilds(await unconfiguredGuilds)
  }

  return { unconfiguredGuilds, configureGuild }
}
