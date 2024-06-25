managed:

call slx api
call SLX_PollErrors
raise SLX Exception

unmanaged:

slx api:
    call other apis
    sets an error on error (error list)

slx:
    use CamelCase for functions
    use SLX_CamelCase for apis