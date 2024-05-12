export interface ActiveGuild {
  guildId: string
  lanId: string | null
}

export type Guild = {
  id: string
  name: string
  icon: string | null
  provider: string
}

export type Lan = {
  id: string
  guildId: string
  active: boolean
  title: string
  background: string
  createdAt: Date
  updatedAt: Date
}

export type User = {
  id: string
  name: string
  avatar: string | null
  roles: Role[]
}

export enum Role {
  SUPERADMIN = "Superadmin",
  ADMIN = "Admin",
  OPERATOR = "Operator",
  USER = "User",
}

export type Seat = {
  id: string
  title: string
  bounds: Bounds
  reservedBy: User | null
}

export type Bounds = {
  x: number
  y: number
  width: number
  height: number
}

export type GuildRole = {
  id: string
  name: string
  color: number
  roles: Role[]
}
