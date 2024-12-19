import { Cancel, CheckBox, CheckBoxOutlineBlank } from "@mui/icons-material"
import {
  Autocomplete,
  Box,
  Button,
  Checkbox,
  Divider,
  IconButton,
  Stack,
  TextField,
  Typography,
} from "@mui/material"
import { GuildRole, GuildRoleMapping, Role } from "../Adapters/Models"
import { useGuild } from "../Adapters/Guilds/Guilds"
import React from "react"

export function GuildRoleOverview(props: { guildId: string }) {
  const { guild, updateGuild } = useGuild(props.guildId)
  const availableGuildRoles = guild.roles.filter(
    (guildRole) => !guild.roleMapping.find((mapping) => mapping.roleId == guildRole.id)
  )

  async function handleAddMappingPressed() {
    const unused = availableGuildRoles[0]
    await updateGuild({
      ...guild,
      roleMapping: [...guild.roleMapping, { roleId: unused.id, roles: [] }],
    })
  }

  async function handleRoleMappingDeleteClick(roleId: string) {
    const filtered = guild.roleMapping.filter((mapping) => mapping.roleId != roleId)
    await updateGuild({
      ...guild,
      roleMapping: filtered,
    })
  }

  async function handleRolesChange(roleId: string, roles: Role[]) {
    const filtered = guild.roleMapping.filter((mapping) => mapping.roleId != roleId)
    await updateGuild({
      ...guild,
      roleMapping: [...filtered, { roleId: roleId, roles: roles }],
    })
  }

  async function handleGuildRoleChange(from: GuildRole, to: GuildRole | null) {
    if (!to) throw new Error("to cannot be null")

    const removed = guild.roleMapping.find((mapping) => mapping.roleId == from.id)
    if (!removed) throw new Error("Cannot find guild id in guild")

    const filtered = guild.roleMapping.filter((mapping) => mapping.roleId != from.id)
    await updateGuild({
      ...guild,
      roleMapping: [...filtered, { roleId: to.id, roles: removed.roles }],
    })
  }

  const renderRoleMappingEntry = (mapping: GuildRoleMapping) => {
    const currentGuildRole = guild.roles.find((role) => role.id == mapping.roleId)
    if (!currentGuildRole) throw new Error("Cannot find guild role id in guild")

    return (
      <Stack
        spacing={1}
        width={"100%"}
        direction={"row"}
        justifyContent={"space-evenly"}
        key={mapping.roleId}
      >
        <Autocomplete
          options={guild.roles}
          autoHighlight
          disableClearable={true}
          sx={{ width: "50%" }}
          value={currentGuildRole}
          onChange={(e, value) => handleGuildRoleChange(currentGuildRole, value)}
          getOptionLabel={(option) => option.name}
          isOptionEqualToValue={(option, value) => option.id == value.id}
          getOptionDisabled={(option) =>
            !availableGuildRoles.find((guildRole) => guildRole.id == option.id)
          }
          renderOption={(props, option) => (
            <Box component="li" sx={{ color: "#" + option.color.toString(16) }} {...props}>
              {option.name}
            </Box>
          )}
          renderInput={(params) => (
            <TextField
              {...params}
              label="Guild role"
              inputProps={{
                ...params.inputProps,
              }}
            />
          )}
        />
        <Autocomplete
          multiple
          disableCloseOnSelect
          limitTags={1}
          options={Object.values(Role).filter((role) => role != Role.SUPERADMIN)}
          autoHighlight
          sx={{ width: "50%" }}
          value={mapping.roles}
          onChange={(e, value) => handleRolesChange(mapping.roleId, value)}
          getOptionLabel={(option) => option.toString()}
          renderOption={(props, option, { selected }) => (
            <li {...props}>
              <Checkbox
                icon={<CheckBoxOutlineBlank fontSize="small" />}
                checkedIcon={<CheckBox fontSize="small" />}
                style={{ marginRight: 8 }}
                checked={selected}
              />
              {option.toString()}
            </li>
          )}
          renderInput={(params) => {
            params.inputProps.children = undefined
            params.inputProps["aria-expanded"] = false
            params.inputProps["aria-controls"] = undefined
            params.inputProps["aria-activedescendant"] = undefined
            return <TextField {...params} label="Role" error={mapping.roles.length == 0} />
          }}
        />
        <IconButton onClick={() => handleRoleMappingDeleteClick(mapping.roleId)}>
          <Cancel />
        </IconButton>
      </Stack>
    )
  }

  return (
    <Stack spacing={2} justifyContent="center" alignItems="center">
      <Typography height={"100%"} variant={"h5"}>
        {guild.name}
      </Typography>
      <Divider sx={{ width: "100%" }} />
      <Stack spacing={2} width={"100%"} justifyContent="center" alignItems="center">
        {guild.roleMapping.length > 0 ? (
          guild.roleMapping.map((mapping) => renderRoleMappingEntry(mapping))
        ) : (
          <Typography>Ingen mappinger registrert</Typography>
        )}
        <Divider />
        <Button
          variant="outlined"
          disabled={availableGuildRoles.length == 0}
          onClick={handleAddMappingPressed}
        >
          Legg til mapping
        </Button>
      </Stack>
    </Stack>
  )
}
