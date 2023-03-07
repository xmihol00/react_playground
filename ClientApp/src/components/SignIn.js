import React, { Component } from 'react';

export class SignIn extends Component {
    static displayName = SignIn.name;

    constructor(props) {
      super(props);
      this.signIn = this.signIn.bind(this);
    }

    async signIn(event) 
    {
        event.preventDefault();
        const form = event.target;
        const data = Array.from(form.elements)
        .filter((input) => input.name)
        .reduce(
            (obj, input) => Object.assign(obj, { [input.name]: input.value }),
            {}
        );

        await fetch("account/signin", 
        {
          method: "POST",
          headers: {
            'Content-type': 'application/json; charset=UTF-8',
          },
          body: JSON.stringify(data),
        })
    }

    render() {
        return (
          <form onSubmit={this.signIn}>
            <label>Name:</label>
            <input type={"text"} name="name" className='form-control' />
            <label>Role:</label>
            <input type={"text"} name="role" className='form-control' />
            <label>Login:</label>
            <input type="text" name='login' className='form-control'/>
            <label>Password:</label>
            <input type={'password'} name="password" className='form-control' />
            <input className="btn btn-primary" type={"submit"} value="Sign In"/>
          </form>
        );
    }
}
