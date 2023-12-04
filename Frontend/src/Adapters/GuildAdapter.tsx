import { useEffect, useState } from "react"
import useApiRequests from "./ApiRequestHook"
import { Role, User } from "./AuthenticationAdapter"
import useVersionId from "./VersionIdHook"

export interface Guild {
  id: string
  name: string
  icon: string | null
}

export interface GuildRole {
  id: string
  name: string
  color: number
  roles: Role[]
}

export function useGuild(guildId: string) {
  const { apiRequest } = useApiRequests()
  const { versionId } = useVersionId("guild_" + guildId)
  const [guild, setGuild] = useState<Guild | null>(null)

  useEffect(() => {
    loadGuild()
  }, [versionId])

  async function loadGuild(): Promise<Guild | null> {
    const response = await apiRequest("GET", `guild/${guildId}`)

    const guild = response.status == 404 ? null : ((await response.json()) as Guild)

    setGuild(guild)
    return guild
  }

  return guild
}

export function useGuildUsers(guildId: string) {
  const { apiRequest } = useApiRequests()
  const [guildUsers, setGuildUsers] = useState<User[] | null>(null)

  async function loadGuildUsers() {
    const response = await apiRequest("GET", `guild/${guildId}/users`)

    const users = response.status == 404 ? null : ((await response.json()) as User[])

    setGuildUsers(users)
  }

  return { guildUsers, loadGuildUsers }
}

export function useGuilds() {
  const { apiRequest } = useApiRequests()
  const { versionId } = useVersionId("guilds")
  const [guilds, setGuilds] = useState<Guild[] | null>(null)

  useEffect(() => {
    loadGuilds()
  }, [versionId])

  async function loadGuilds(): Promise<Guild[]> {
    const response = await apiRequest("GET", `guild`)

    const guilds = (await response.json()) as Guild[]
    setGuilds(guilds)
    return guilds
  }

  return guilds
}

export function useGuildRoles(guildId: string) {
  const { apiRequest } = useApiRequests()
  const { versionId, invalidate } = useVersionId("guild_roles_" + guildId)
  const [guildRoles, setGuildRolesState] = useState<GuildRole[] | null>(null)

  useEffect(() => {
    loadGuildRoles()
  }, [versionId, guildId])

  const loadGuildRoles = async () => {
    const response = await apiRequest("GET", `guild/${guildId}/roles`)

    const roles = (await response.json()) as GuildRole[]
    setGuildRolesState(roles)
  }

  const setGuildRoles = async (guild: Guild, guildRoles: GuildRole[]) => {
    await apiRequest("PUT", `guild/${guild.id}/roles`, guildRoles)
    invalidate()
  }

  return { guildRoles, setGuildRoles }
}
