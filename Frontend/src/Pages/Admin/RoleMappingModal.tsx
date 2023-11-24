/* eslint-disable @typescript-eslint/no-unused-vars */
import {
  Autocomplete,
  Box,
  Button,
  Container,
  Divider,
  IconButton,
  Modal,
  Stack,
  TextField,
  Typography,
} from "@mui/material"
import {
  Guild,
  GuildRole,
  GuildRoleMapping,
  useGuildAdapter,
  useGuildRoles,
} from "../../Adapters/GuildAdapter"
import { Add } from "@mui/icons-material"
import DelayedCircularProgress from "../../Components/DelayedCircularProgress"
import { Role } from "../../Adapters/AuthenticationAdapter"
import { useEffect, useState } from "react"

function Line(props: {
  roles: GuildRole[]
  selectedGuildRoleId: string | null
  selectedRole: Role | null
  onSelectedRoleChanged: (role: GuildRole) => void
  onSelectedGuildRoleChanged: (guildRole: Role) => void
}) {
  return (
    <Stack
      spacing={1}
      width={"100%"}
      direction={"row"}
      justifyContent={"space-evenly"}
    >
      <Autocomplete
        options={props.roles}
        autoHighlight
        getOptionLabel={(option) => option.name}
        renderOption={(props, option) => (
          <Box component="li" {...props}>
            {option.name}
          </Box>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            fullWidth
            size="small"
            label="Guild role"
            inputProps={{
              ...params.inputProps,
            }}
          />
        )}
      />
      <Autocomplete
        options={Object.values(Role)}
        autoHighlight
        getOptionLabel={(option) => option.toString()}
        renderOption={(props, option) => (
          <Box component="li" {...props}>
            {option.toString()}
          </Box>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            fullWidth
            size="small"
            label="Role"
            inputProps={{
              ...params.inputProps,
            }}
          />
        )}
      />
    </Stack>
  )
}

export function RoleMappingModal(props: { guild: Guild; open: boolean }) {
  const { setGuildRoleMapping } = useGuildAdapter()
  const { roles, roleMappings } = useGuildRoles(props.guild.id)
  const [stagedRoleMappings, setStagedRoleMappings] = useState<
    GuildRoleMapping[] | null
  >(null)

  useEffect(() => {
    if (!stagedRoleMappings && roleMappings) {
      setStagedRoleMappings(roleMappings)
    }
  }, [roleMappings])

  function handleAddMappingPressed() {
    if (stagedRoleMappings)
      setStagedRoleMappings([
        { role: null, roleId: null },
        ...stagedRoleMappings,
      ])
  }

  function handleGuildRoleSelected(guildRole: string | null) {}

  function handleRoleSelected(role: Role | null) {}

  return (
    <Modal {...props}>
      <Box
        sx={{
          position: "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          minWidth: "60%",
          bgcolor: "background.paper",
          p: 4,
        }}
      >
        <Stack spacing={1} justifyContent="center" alignItems="center">
          <Typography height={"100%"} variant={"h5"}>
            {props.guild.name}
          </Typography>
          <Divider />
          {stagedRoleMappings && roles ? (
            <Stack spacing={1} width={"100%"}>
              {stagedRoleMappings.map((mapping, i) => (
                <Line
                  key={i}
                  roles={roles}
                  selectedRole={mapping.role}
                  selectedGuildRoleId={mapping.roleId}
                  onSelectedGuildRoleChanged={() =>
                    handleGuildRoleSelected(mapping.roleId)
                  }
                  onSelectedRoleChanged={() => handleRoleSelected(mapping.role)}
                />
              ))}
              <Button variant="outlined" onClick={handleAddMappingPressed}>
                Add mapping
              </Button>
            </Stack>
          ) : (
            <Stack width="100%" justifyContent="center" alignItems="center">
              <DelayedCircularProgress />
            </Stack>
          )}
        </Stack>
      </Box>
    </Modal>
  )
}
