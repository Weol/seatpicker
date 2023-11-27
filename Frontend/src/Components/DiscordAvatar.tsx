import { Avatar, AvatarProps } from "@mui/material"
import * as React from "react"
import Config from "../config"
import { User } from "../Adapters/AuthenticationAdapter"
import { Guild } from "../Adapters/GuildAdapter"

function stringToColor(string: string) {
  let hash = 0
  let i

  for (i = 0; i < string.length; i += 1) {
    hash = string.charCodeAt(i) + ((hash << 5) - hash)
  }

  let color = "#"

  for (i = 0; i < 3; i += 1) {
    const value = (hash >> (i * 8)) & 0xff
    color += `00${value.toString(16)}`.slice(-2)
  }

  return color
}

function stringAvatar(name: string, props: AvatarProps) {
  return {
    ...props,
    sx: {
      bgcolor: stringToColor(name),
      ...props.sx,
    },
    children: `${name[0]}`,
  }
}

export function DiscordUserAvatar(props: { user: User } & AvatarProps) {
  return props.user.avatar != null ? (
    <Avatar
      alt={props.user.name}
      src={
        Config.DiscordAvatarBaseUrl + props.user.id + "/" + props.user.avatar
      }
      {...props}
    />
  ) : (
    <Avatar {...stringAvatar(props.user.name, props)} />
  )
}

export default function DiscordGuildAvatar(
  props: { guild: Guild } & AvatarProps
) {
  return props.guild.icon != null ? (
    <Avatar
      alt={props.guild.name}
      src={
        Config.DiscordGuildIconBaseUrl + props.guild.id + "/" + props.guild.icon
      }
      {...props}
    />
  ) : (
    <Avatar {...props} {...stringAvatar(props.guild.name, props)} />
  )
}
