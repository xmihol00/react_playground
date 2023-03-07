import Cookies from 'js-cookie'; //-- npm i js-cookie

export function checkRole(role)
{
    return Cookies.get("roles").includes(role);
}

export function getRoles()
{
    return Cookies.get("roles").split(',');
}
