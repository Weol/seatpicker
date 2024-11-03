import {atom, useRecoilValue} from "recoil"
import ApiRequest from "../ApiRequest"
import {UnconfiguredGuild} from "../Models"

async function loadAllUnconfiguredGuilds() {
  const response = await ApiRequest("GET", `guild/unconfigured`)
  return (await response.json()) as UnconfiguredGuild[]
}

export const allUnconfiguredGuildsAtom = atom<UnconfiguredGuild[]>({
  key: "allUnconfiguredGuilds",
  effects: [({ setSelf }) => setSelf(loadAllUnconfiguredGuilds())],
})

export function useUnconfiguredGuilds() {
  const unconfiguredGuilds = useRecoilValue(allUnconfiguredGuildsAtom)

  const configureGuild = async (unconfiguredGuild: UnconfiguredGuild) => {
    await ApiRequest("PUT", `guild/${unconfiguredGuild.id}`, {
      id: unconfiguredGuild.id,
      name: unconfiguredGuild.name,
      icon: unconfiguredGuild.icon,
      hostNames: [],
      roleMapping: []
    })
  }

  return { unconfiguredGuilds, configureGuild }
}