$characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!#$%&()*+,-./:;<=>?@[\]^_`{|}~".ToCharArray()
$length= (30..60 | Get-Random) 

$passwordList = @()
For ($i = 0; $i -lt $length; $i++) {
   $passwordList += $characters | Get-Random
}
$password = -join $passwordList

$DeploymentScriptOutputs = @{}
$DeploymentScriptOutputs['password'] = $password