/* eslint-disable @typescript-eslint/no-unused-vars */
import {
  Autocomplete,
  Box,
  Button,
  Container,
  Divider, Icon,
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
import {Add, Cancel} from "@mui/icons-material"
import DelayedCircularProgress from "../../Components/DelayedCircularProgress"
import {Role} from "../../Adapters/AuthenticationAdapter"
import {useEffect, useState} from "react"

function Line(props: {
  key: number,
  roles: GuildRole[]
  selectedGuildRoleId: string | null
  selectedRole: Role | null
  onSelectedRoleChanged: (key: number, role: Role) => void
  onSelectedGuildRoleChanged: (key: number, guildRole: GuildRole) => void
  onDeleteLine: (key: number) => void
})
{
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
        sx={{ width: "50%" }}
        getOptionLabel={(option) => option.name}
        renderOption={(props, option) => (
          <Box component="li" {...props}>
            {option.name}
          </Box>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
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
        sx={{ width: "50%" }}
        getOptionLabel={(option) => option.toString()}
        renderOption={(props, option) => (
          <Box component="li" {...props}>
            {option.toString()}
          </Box>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            size="small"
            label="Role"
            inputProps={{
              ...params.inputProps,
            }}
          />
        )}
      />
      <IconButton onClick={() => props.onDeleteLine(props.key)}><Cancel /></IconButton>
    </Stack>
  )
}

export function RoleMappingModal(props: { guild: Guild; open: boolean }) {
  const { setGuildRoleMapping } = useGuildAdapter()
  const { roles, roleMappings } = useGuildRoles(props.guild.id)
  const [ stagedRoleMappings, setStagedRoleMappings ] = useState<GuildRoleMapping[] | null>(null)

  useEffect(() => {
    if (!stagedRoleMappings && roleMappings) {
      setStagedRoleMappings(roleMappings)
    }
  }, [ roleMappings ])

  function handleAddMappingPressed() {
    if (stagedRoleMappings)
      setStagedRoleMappings([
        { role: null, roleId: null },
        ...stagedRoleMappings,
      ])
  }

  function handleGuildRoleSelected(key: number, guildRole: GuildRole | null) {
  }

  function handleRoleSelected(key: number, role: Role | null) {
  }

  function handleLineDeleted(key: number) {
  }

  return (
    <Modal {...props}>
      <Box
        sx={{
          position: "absolute",
          top: "50%",
          left: "50%",
          transform: "translate(-50%, -50%)",
          width: [ "90%", "50%" ],
          bgcolor: "background.paper",
          p: 4,
        }}
      >
        <Stack spacing={2} justifyContent="center" alignItems="center">
          <Stack direction={"row"} width="100%" justifyContent="space-between" alignItems="center">
            <Typography height={"100%"} variant={"h5"}>
              {props.guild.name}
            </Typography>
            <Button variant={"outlined"}>Lagre</Button>
          </Stack>
          <Divider sx={{ width: "100%" }}/>
          {stagedRoleMappings && roles ? (
            <Stack spacing={2} width={"100%"} justifyContent="center" alignItems="center">
              {stagedRoleMappings.length > 0 ? stagedRoleMappings.map((mapping, i) => (
                <Line
                  key={i}
                  roles={roles}
                  selectedRole={mapping.role}
                  selectedGuildRoleId={mapping.roleId}
                  onSelectedGuildRoleChanged={(key, guildRole) =>
                    handleGuildRoleSelected(key, guildRole)
                  }
                  onSelectedRoleChanged={(key, role) => handleRoleSelected(key, role)}
                  onDeleteLine={(key) => handleLineDeleted(key)}/>
              )) : (
                <Typography>Ingen mappinger registrert</Typography>
              )}
              <Divider/>
              <Button variant="outlined" onClick={handleAddMappingPressed}>
                Legg til mapping
              </Button>
            </Stack>
          ) : (
            <Stack width="100%" justifyContent="center" alignItems="center">
              <DelayedCircularProgress/>
            </Stack>
          )}
        </Stack>
      </Box>
    </Modal>
  )
}
