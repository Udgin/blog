describe('Check all links', function() {
    it('Visit links at pages', function() {
        cy.server()
        checkFolder('https://eapyl.github.io/')
        //checkFolder('http://127.0.0.1:8080/')

        function checkFolder(uri) {
            cy.visit(uri)
            cy.get('div.columns').then(($container) => {
                if ($container.find('code:not(:empty)').length)
                {
                    cy.get('code:not(:empty)').parent().find('a').each(($a) => {
                        checkArticle($a[0].href)
                        //cy.route($a[0].href).its('status').should('eq', 200)
                    })
                }
            })

            cy.visit(uri)

            cy.get('div.columns').then(($container) => {
                if ($container.find('code:empty').length)
                {
                    cy.get('code:empty').parent().find('a').each(($a) => {
                        checkFolder($a[0].href)
                    })
                }
            })
        }

        function checkArticle(uri)
        {
            cy.visit(uri)
            cy.get('article').then(($article) => {
                if ($article.find('a').length)
                {
                    cy.get('article a').each(($a) => {
                        cy.route($a[0].href).its('status').should('eq', 200)
                    })
                }
            })
        }
    })
  })